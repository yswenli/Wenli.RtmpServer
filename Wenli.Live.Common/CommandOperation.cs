using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wenli.Live.Common
{
    public enum CommandOperation : int
    {
        Subscribe = 0,
        Unsubscribe = 1,
        Poll = 2, // poll for undelivered messages
        DataUpdateAttributes = 3,
        ClientSync = 4, // sent by remote to sync missed or cached messages to a client as a result of a client issued poll command
        ClientPing = 5, // connectivity test
        DataUpdate = 7,
        ClusterRequest = 7, // request a list of failover endpoint URIs for the remote destination based on cluster membership
        Login = 8,
        Logout = 9,
        InvalidateSubscription = 10, // indicates that client subscription has been invalidated (eg timed out)
        ChannelDisconnected = 12, // indicates that a channel has disconnected
        Unknown = 10000
    }
}
