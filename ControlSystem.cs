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
        private GenericComponent levelGetter, levelGetter2;
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
                CrestronConsole.AddNewConsoleCommand(Dsp, "dsp", "send command", ConsoleAccessLevelEnum.AccessOperator);
                var biamp = new BiampTesira();
                biamp.Initialize(1);
                biamp.OnCommunicatingChange += OnCommunicatingChange;
                biamp.Configure(1, 1, "192.168.2.35", "admin", "admin");
                biamp.Connect();

                CrestronConsole.PrintLine("SystemStart!");

                var state1 = new StateComponent();
                state1.Configure(1, "Program_Audio", "mute", 1, 0);

                var state2 = new StateComponent();
                state2.Configure(1, "Room_Audio", "mute", 1, 0);

                levelGetter = new GenericComponent();
                levelGetter2 = new GenericComponent();
                levelGetter.Configure(1, "Program_Audio", "minLevel", 1, 0, 2, 1);
                levelGetter2.Configure(1, "Program_Audio", "maxLevel", 1, 0, 2, 1);

                state1.OnStateChange += State1OnOnStateChange;
                state2.OnStateChange += State1OnOnStateChange;
                levelGetter.OnSerialChange += LevelGetterMinOnSerialChange;
                levelGetter.OnAnalogChangeSigned += LevelGetterOnOnAnalogChangeSigned;
                levelGetter2.OnSerialChange += LevelGetterMaxOnSerialChange;
                

            }
            catch (Exception e )
            {
                CrestronConsole.PrintLine(e.ToString());
            }
            return null;
        }

        private void LevelGetterOnOnAnalogChangeSigned(object sender, Int16EventArgs args)
        {
            CrestronConsole.PrintLine("LevelGetterMinOnAnalogChange Int. args:{0}", args.Payload);
        }

        private void LevelGetterMaxOnSerialChange(object sender, StringEventArgs args)
        {
            CrestronConsole.PrintLine("LevelGetterMaxOnSerialChange. args:{0}", args.Payload);
        }
        
        private void LevelGetterMinOnSerialChange(object sender, StringEventArgs args)
        {
            CrestronConsole.PrintLine("LevelGetterMinOnSerialChange. args:{0}", args.Payload);
        }

        private void Dsp(string cmdparameters)
        {
            if (cmdparameters.Length <= 0) return;
            levelGetter.SetSerial(cmdparameters); // i.e. Program_Audio get minLevel 1
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