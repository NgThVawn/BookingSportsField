using System.Collections.Concurrent;
using BookingSportsField.Models;
using Microsoft.AspNetCore.SignalR;

namespace BookingSportsField.Hubs
{
    public class BookingHub : Hub
    {
        private static ConcurrentDictionary<string, string> TempLocks = new();
        public override async Task OnConnectedAsync()
        {
            // Gắn UserId vào group riêng
            if (Context.UserIdentifier != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, Context.UserIdentifier);
            }
            // Tự động join group theo role để nhận broadcast
            var user = Context.User;
            if (user?.IsInRole(SD.Role_Admin) == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, SD.Role_Admin);
            }
            if (user?.IsInRole(SD.Role_FieldOwner) == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, SD.Role_FieldOwner);
            }
            await base.OnConnectedAsync();
        }

        // Gửi thông báo cho 1 user cụ thể
        public async Task NotifyUser(string userId, string message)
        {
            await Clients.Group(userId).SendAsync("ReceiveNotification", message);
        }

        // Gửi thông báo cho 1 group (ví dụ Admin / FieldOwner)
        public async Task NotifyGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveNotification", message);
        }

        // Join group theo role
        public async Task JoinRoleGroup(string role)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, role);
        }
        public async Task HoldSlot(string slotKey)
        {
            TempLocks[slotKey] = Context.ConnectionId;
            await Clients.Others.SendAsync("SlotLocked", slotKey);
        }

        public async Task ReleaseSlot(string slotKey)
        {
            TempLocks.TryRemove(slotKey, out _);
            await Clients.Others.SendAsync("SlotReleased", slotKey);
        }

        public bool IsSlotLocked(string slotKey)
        {
            return TempLocks.ContainsKey(slotKey);
        }

        public override Task OnDisconnectedAsync(Exception? ex)
        {
            var locks = TempLocks.Where(x => x.Value == Context.ConnectionId).ToList();
            foreach (var l in locks)
            {
                TempLocks.TryRemove(l.Key, out _);
            }
            return base.OnDisconnectedAsync(ex);
        }
    }
}
