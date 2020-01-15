using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DemoAngularApp.Hubs
{
    public class Notification : Hub
    {
        public override System.Threading.Tasks.Task OnConnected()
        {
             
            return base.OnConnected();
        }

        

    }
}