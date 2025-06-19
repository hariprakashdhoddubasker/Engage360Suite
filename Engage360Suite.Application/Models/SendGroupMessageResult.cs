using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engage360Suite.Application.Models
{
    public class SendGroupMessageResult
    {
        public string Status { get; set; } = default!;
        public GroupMessageInfo Message { get; set; } = default!;
    }

    public class GroupMessageInfo
    {
        public MessageKey Key { get; set; } = default!;
        public ExtendedTextContainer Message { get; set; } = default!;
        public string MessageTimestamp { get; set; } = default!;
        public string Participant { get; set; } = default!;
    }

    public class MessageKey
    {
        public string RemoteJid { get; set; } = default!;
        public bool FromMe { get; set; }
        public string Id { get; set; } = default!;
    }

    public class ExtendedTextContainer
    {
        public TextPayload ExtendedTextMessage { get; set; } = default!;
    }

    public class TextPayload
    {
        public string Text { get; set; } = default!;
    }
}
