using System.Collections.Generic;
using System.Linq;
using SteamKit2;
using SteamKit2.Internal;

namespace SteamBot
{
    public class PacketMsgHandler : ClientMsgHandler
    {
        public override void HandleMsg(IPacketMsg PacketMsg)
        {
            switch (PacketMsg.MsgType)
            {
                case EMsg.ClientPurchaseResponse:
                    HandleClientPurchaseResponse(PacketMsg);
                    break;
                case EMsg.ClientItemAnnouncements:
                    HandleClientItemAnnouncements(PacketMsg);
                    break;
                case EMsg.ClientUserNotifications:
                    HandleClientUserNotifications(PacketMsg);
                    break;
            }
        }

        public class ClientPurchaseResponse : CallbackMsg
        {
            public JobID SourceJobID { get; private set; }

            public JobID TargetJobID { get; private set; }

            public EResult Result { get; private set; }

            public EPurchaseResultDetail Purchase_Result_Detail { get; private set; }

            public byte[] Purchase_Receipt_Info { get; private set; }

            internal ClientPurchaseResponse(JobID jobid_source, JobID jobid_target, EResult eresult, EPurchaseResultDetail purchase_result_details, byte[] purchase_receipt_info)
            {
                SourceJobID = jobid_source;
                TargetJobID = jobid_target;
                Result = eresult;
                Purchase_Result_Detail = purchase_result_details;
                Purchase_Receipt_Info = purchase_receipt_info;
            }
        }

        public class ClientItemAnnouncements : CallbackMsg
        {
            public JobID SourceJobID { get; private set; }

            public JobID TargetJobID { get; private set; }

            public uint Count_New_Items { get; private set; }

            internal ClientItemAnnouncements(JobID jobid_source, JobID jobid_target, uint count_new_items)
            {
                SourceJobID = jobid_source;
                TargetJobID = jobid_target;
                Count_New_Items = count_new_items;
            }
        }

        public class ClientUserNotifications : CallbackMsg
        {
            public List<Notification> Notifications { get; private set; }

            internal ClientUserNotifications(List<CMsgClientUserNotifications.Notification> notifications)
            {
                Notifications = notifications.Select(x => new Notification(x)).ToList();
            }

            public sealed class Notification
            {
                public uint Count { get; private set; }

                public uint User_Notification_Type { get; private set; }

                internal Notification(CMsgClientUserNotifications.Notification notification)
                {
                    Count = notification.count;
                    User_Notification_Type = notification.user_notification_type;
                }
            }
        }

        private void HandleClientPurchaseResponse(IPacketMsg PacketMsg)
        {
            ClientMsgProtobuf<CMsgClientPurchaseResponse> ClientPurchaseResponse = new ClientMsgProtobuf<CMsgClientPurchaseResponse>(PacketMsg);

            Client.PostCallback(new ClientPurchaseResponse(
                PacketMsg.SourceJobID,
                PacketMsg.TargetJobID,
                (EResult)ClientPurchaseResponse.Body.eresult,
                (EPurchaseResultDetail)ClientPurchaseResponse.Body.purchase_result_details,
                ClientPurchaseResponse.Body.purchase_receipt_info
            ));
        }

        private void HandleClientItemAnnouncements(IPacketMsg PacketMsg)
        {
            ClientMsgProtobuf<CMsgClientItemAnnouncements> ClientItemAnnouncements = new ClientMsgProtobuf<CMsgClientItemAnnouncements>(PacketMsg);

            Client.PostCallback(new ClientItemAnnouncements(
                ClientItemAnnouncements.SourceJobID,
                ClientItemAnnouncements.TargetJobID,
                ClientItemAnnouncements.Body.count_new_items
            ));
        }

        private void HandleClientUserNotifications(IPacketMsg PacketMsg)
        {
            ClientMsgProtobuf<CMsgClientUserNotifications> ClientUserNotifications = new ClientMsgProtobuf<CMsgClientUserNotifications>(PacketMsg);
            Client.PostCallback(new ClientUserNotifications(ClientUserNotifications.Body.notifications));
        }
    }
}
