using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailEZ.Application.Features.Dashboard.Dto
{
    public class DashboardDto
    {
        public Guid UserId { get; set; }
        public int EmailsSent { get; set; }
        public int EmailsReceived { get; set; } = 0;
        public int EmailsDrafted { get; set; } = 0;
        public int EmailsScheduled { get; set; } = 0;
        public int EmailsFailed { get; set; } = 0;
        public int EmailsOpened { get; set; } = 0;
        public int EmailsClicked { get; set; } = 0;
        public int EmailsUnsubscribed { get; set; } = 0;
        public int EmailsBounced { get; set; } = 0;
        public int EmailsMarkedAsSpam { get; set; } = 0;
        public int EmailsReplied { get; set; } = 0;
        public int EmailsForwarded { get; set; } = 0;
        public int EmailsArchived { get; set; } = 0;
        public int EmailsDeleted { get; set; } = 0;
        public int EmailsPending { get; set; } = 0;
        public int EmailsInProgress { get; set; } = 0;
        public int EmailsTotal { get; set; } = 0;
    }
}
