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
        private LevelComponent level1;
        private StateComponent _state1;

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
                CrestronConsole.AddNewConsoleCommand(Poll, "poll", "send command", ConsoleAccessLevelEnum.AccessOperator);
                var biamp = new BiampTesira();
                biamp.Initialize(1);
                biamp.OnCommunicatingChange += OnCommunicatingChange;
                biamp.Configure(1, 1, "192.168.2.35", "admin", "admin");
                biamp.Connect();

                CrestronConsole.PrintLine("SystemStart");

                _state1 = new StateComponent();
                _state1.Configure(1, "Program_Audio", "mute", 1, 0);

                var state2 = new StateComponent();
                state2.Configure(1, "Room_Audio", "mute", 1, 0);

                level1 = new LevelComponent();
                level1.Configure(1, "Program_Audio", "level", 1, 0, 3);

                levelGetter = new GenericComponent();
                levelGetter.Configure(1, "Program", "mute", 1, 0, 0, 0);
                
                //levelGetter2 = new GenericComponent();
                //levelGetter2.Configure(1, "Program_Audio", "maxLevel", 1, 0, 1, 0);

                _state1.OnStateChange += State1OnOnStateChange;
                state2.OnStateChange += State1OnOnStateChange;
                
                //level1.OnLevelChangeSignedUnscaled += Level1OnOnLevelChangeSignedUnscaled;
                //level1.OnLevelTextChange += Level1OnOnLevelTextChange;
                
                levelGetter.OnDigitalChange += LevelGetterOnOnDigitalChange;
                //levelGetter.OnAnalogChangeSigned += LevelGetterOnOnAnalogChangeSigned;
                //levelGetter2.OnAnalogChangeSigned += LevelGetterOnOnAnalogChangeSigned;
                

            }
            catch (Exception e )
            {
                CrestronConsole.PrintLine(e.ToString());
            }
            return null;
        }

        private void LevelGetterOnOnDigitalChange(object sender, UInt16EventArgs args)
        {
            CrestronConsole.PrintLine("LevelGetterOnOnDigitalChange. args:{0}", args.Payload);
        }

        private void Level1OnOnLevelTextChange(object sender, StringEventArgs args)
        {
            var lc = (LevelComponent)sender;
            CrestronConsole.PrintLine("Level1OnOnLevelTextChange {0}. args:{1}", lc.ConfigInfo.InstanceTag, args.Payload);
        }

        private void Level1OnOnLevelChangeSignedUnscaled(object sender, Int16EventArgs args)
        {
            var lc = (LevelComponent)sender;
            CrestronConsole.PrintLine("Level1OnOnLevelChangeSignedUnscaled {0}. args:{1}", lc.ConfigInfo.InstanceTag, args.Payload);
        }

        private void LevelGetterOnOnAnalogChangeSigned(object sender, Int16EventArgs args)
        {
            //var lc = (LevelComponent)sender;
            CrestronConsole.PrintLine("LevelGetterMinOnAnalogChange {0}.7}", /*lc.ConfigInfo.InstanceTag, */args.Payload);
        }

        private void Poll(string cmdparameters)
        {
            if (cmdparameters.Length <= 0) return;
            switch (cmdparameters)
            {
                case "min":
                    levelGetter.Poll();
                    break;
                case "max":
                    levelGetter2.Poll();
                    break;
                case "level":
                    level1.Poll();
                    break;
                case "levels":
                    level1.PollState();
                    break;
                case "state":
                    levelGetter.Poll();
                    break;
                default:
                    CrestronConsole.PrintLine("Invalid args");
                    break;
            }
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