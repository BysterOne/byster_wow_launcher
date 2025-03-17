using Launcher.Api.Models;
using Launcher.Settings.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Settings
{
    class GProp
    {
        public static RReferralSource? ReferralSource { get; set; } = null;
        public static EServer Server { get; set; } = EServer.Prod;
        public static JsonSerializerSettings JsonSeriSettings { get => new() { NullValueHandling = NullValueHandling.Ignore }; }
    }
}
