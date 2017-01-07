﻿using ComputerPlus.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerPlus
{
    internal sealed class Globals
    {
        internal static Random Random = new Random();
        internal static bool IsPlayerOnDuty = false;
        internal static List<CalloutData> CallQueue = new List<CalloutData>();
        internal static Guid ActiveCallID = Guid.Empty;
        internal static bool IsCalloutActive = false;
        internal static int WebAPIFileId = 11453;
        internal static Guid ActiveExternalUI_ID = Guid.Empty;
        internal static List<ExternalUI> ExternalUI = new List<API.ExternalUI>();

        /// <summary>
        /// Returns the active callout from the queue.
        /// This property is readonly, and should NOT be used for updating data.
        /// </summary>
        internal static CalloutData ActiveCallout
        {
            get
            {
                if (ActiveCallID == Guid.Empty)
                    return null;
                else
                    return (from x in CallQueue where x.ID == ActiveCallID select x).FirstOrDefault();
            }
        }

        internal static IOrderedEnumerable<ExternalUI> SortedExternalUI
        {
            get
            {
                return ExternalUI.OrderBy(x => x.DisplayName);
            }
        }

        internal static ExternalUI ActiveExternalUI
        {
            get
            {
                if (ActiveExternalUI_ID == Guid.Empty)
                    return null;
                return ExternalUI.Where(x => x.Identifier == ActiveExternalUI_ID).First();
            }
        }
    }
}