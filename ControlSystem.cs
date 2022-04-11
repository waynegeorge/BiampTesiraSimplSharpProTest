using System;
using BiampTesiraLib3;
using BiampTesiraLib3.Components;
using BiampTesiraLib3.Events;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading
using Crestron.SimplSharpPro.Diagnostics;		    	// For System Monitor Access
using Crestron.SimplSharpPro.DeviceSupport;         	// For Generic Device Support
using Crestron.SimplSharp.Net;
using System.Net.Sockets;

namespace TestSharpPro
{
    public class ControlSystem : CrestronControlSystem
    {
        /// <summary>
        /// ControlSystem Constructor. Starting point for the SIMPL#Pro program.
        /// Use the constructor to:
        /// * Initialize the maximum number of threads (max = 400)
        /// * Register devices
        /// * Register event handlers
        /// * Add Console Commands
        /// 
        /// Please be aware that the constructor needs to exit quickly; if it doesn't
        /// exit in time, the SIMPL#Pro program will exit.
        /// 
        /// You cannot send / receive data in the constructor
        /// </summary>
        public ControlSystem()
            : base()
        {
            try
            {
                Thread.MaxNumberOfUserThreads = 20;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in the constructor: {0}", e.Message);
            }
        }

        /// <summary>
        /// InitializeSystem - this method gets called after the constructor 
        /// has finished. 
        /// 
        /// Use InitializeSystem to:
        /// * Start threads
        /// * Configure ports, such as serial and verisports
        /// * Start and initialize socket connections
        /// Send initial device configurations
        /// 
        /// Please be aware that InitializeSystem needs to exit quickly also; 
        /// if it doesn't exit in time, the SIMPL#Pro program will exit.
        /// </summary>
        public override void InitializeSystem()
        {
            var startThread = new Thread(SystemStart, null, Thread.eThreadStartOptions.Running)
            {
                Priority = Thread.eThreadPriority.HighPriority
            };
            
        }

        internal object SystemStart(object userObject)
        {
            try
            {
                var biamp = new BiampTesira();
                biamp.Initialize(1);
                biamp.OnCommunicatingChange += OnCommunicatingChange;
                biamp.Configure(1, 1, "192.168.2.35", "admin", "admin");
                biamp.Connect();

                CrestronConsole.PrintLine("SystemStart!");

                var state1 = new StateComponent();
                state1.Configure(1, "PGM_Spk_Level", "mute", 1, 0);

                var state2 = new StateComponent();
                state2.Configure(1, "Ceiling_Spk_Level", "mute", 1, 0);

                state1.OnStateChange += State1OnOnStateChange;
                state2.OnStateChange += State1OnOnStateChange;


            }
            catch (Exception e )
            {
                CrestronConsole.PrintLine(e.ToString());
            }
            return null;
        }

        private void State1OnOnStateChange(object sender, UInt16EventArgs args)
        {
            try
            {
                CrestronConsole.PrintLine("State1OnOnStateChange. args:{0}", args.Payload);

                var sc = (StateComponent)sender;

                CrestronConsole.PrintLine("State1OnOnStateChange.  {0}", sc.ConfigInfo.InstanceTag);
            }
            catch(Exception e)
            {
                CrestronConsole.PrintLine(e.ToString());
            }
        }

        private void OnCommunicatingChange(object sender, UInt16EventArgs args)
        {
            CrestronConsole.PrintLine("Biamp Communicating! object is {0}", sender.ToString());
        }
    }
}