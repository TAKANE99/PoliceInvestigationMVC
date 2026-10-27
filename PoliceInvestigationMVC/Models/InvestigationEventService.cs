using System;

namespace PoliceInvestigationMVC.Models
{
    public class InvestigationEventService
    {
        public event EventHandler? InvestigationCreated;

        public void NotifyInvestigationCreated()
        {
            InvestigationCreated?.Invoke(this, EventArgs.Empty);
        }
    }
}