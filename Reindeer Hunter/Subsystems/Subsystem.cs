using Reindeer_Hunter.Hunt;
using System;

namespace Reindeer_Hunter.Subsystems
{
    /// <summary>
    /// Template class for all subsytems.
    /// </summary>
    public class Subsystem
    {
        protected CommandManager Manager { get; set; }

        // Event called whenever the manager property is set
        protected event EventHandler ManagerSet;

        public CommandManager ManagerProperty
        {
            get
            {
                return Manager;
            }
            set
            {
                Manager = value;
                school = value._School;
                ManagerSet(this, new EventArgs());
            }
        }

        protected Subsystem()
        {
            ManagerSet += OnManagerSet;
        }

        protected School school;

        protected void OnManagerSet(object sender, EventArgs e)
        {
            /* When the manager is set, subscribe to the notification for when
             * it gets the home page object. */
            Manager.ParentPageSet += OnHomePageSet;
        }


        protected virtual void OnHomePageSet(object sender, EventArgs e)
        {
            // Set the school property now that we can
            school = Manager._School;

            // Do stuff.
        }
    }
}
