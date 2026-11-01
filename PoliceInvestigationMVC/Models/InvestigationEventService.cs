using System;

namespace PoliceInvestigationMVC.Models
{
    public class InvestigationEventService
    {
        public event EventHandler<Investigation>? InvestigationCreated;

        public void NotifyInvestigationCreated(Investigation investigation)
        {
            InvestigationCreated?.Invoke(this, investigation);
        }
    }
}