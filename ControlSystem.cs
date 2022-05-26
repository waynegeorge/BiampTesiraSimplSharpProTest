using System;
using BiampTesiraLib3;
using BiampTesiraLib3.Components;
using BiampTesiraLib3.Events;
using BiampTesiraLib3.Tesira_Support;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread;        	// For Threading

namespace TestSharpPro
{
    public class ControlSystem : CrestronControlSystem
    {
        private BiampTesira biamp;
        private GenericComponent levelGetter, levelGetter2;
        private LevelComponent level1;
        private StateComponent _mic1;
        private StateComponent _mic2;
        private StateComponent _allMics;
        private VoipDialerComponent component1;
        private VoipDialerComponent component2;
        private VoipDialerStatus _status;
        private VoipCallStatus callStatus;
        private int initState;
        private PresetComponent _presets;

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
                CrestronConsole.AddNewConsoleCommand(Cmd, "cmd", "send command", ConsoleAccessLevelEnum.AccessOperator);
                biamp = new BiampTesira();
                biamp.Initialize(1);
                biamp.OnCommunicatingChange += OnCommunicatingChange;
                biamp.Configure(1, 1, "192.168.2.35", "admin", "admin");
                biamp.Connect();

                CrestronConsole.PrintLine("SystemStart");

                _presets = new PresetComponent();
                _presets.Configure(1);

                _allMics = new StateComponent();
                _allMics.Configure(1, "All_Mics", "mute", 1, 0);
                _allMics.OnStateChange += OnOnStateChange;

                _mic1 = new StateComponent();
                _mic1.Configure(1, "Mic_Mutes", "mute", 1, 0);
                _mic1.OnStateChange += OnOnStateChange;
                //_mic1.OnStateChange += OnOnStateChange1;

                _mic2 = new StateComponent();
                _mic2.Configure(1, "Mic_Mutes", "mute", 2, 0);
                _mic2.OnStateChange += OnOnStateChange;
                //_mic2.OnStateChange += OnOnStateChange2;

                levelGetter = new GenericComponent();
                levelGetter.Configure(1, "TestMic1", "present", 1, 0, 0, 1);
                levelGetter.OnDigitalChange += LevelGetterOnOnDigitalChange;

                levelGetter2 = new GenericComponent();
                levelGetter2.Configure(1, "TestMic2", "present", 1, 0, 0, 1);
                levelGetter2.OnDigitalChange += LevelGetterOnOnDigitalChange;

                
                //_allMics.OnStateChange += OnOnStateChangeAll;
                 
            }
            catch (Exception e )
            {
                CrestronConsole.PrintLine(e.ToString());
            }
            return null;
        }

        private void LevelGetterOnOnDigitalChange(object sender, UInt16EventArgs args)
        {
            if(args.Payload == 0)return;
            var sndr = (GenericComponent)sender;
            CrestronConsole.PrintLine("sndr.ID:{0}", sndr.ID);
            CrestronConsole.PrintLine("InstanceTag:{0}", sndr.ConfigInfo.InstanceTag);
            CrestronConsole.PrintLine("args:{0}\n", args.Payload);

        }

        private void OnOnStateChange(object sender, UInt16EventArgs args)
        {
            var sndr = (StateComponent)sender;
            CrestronConsole.PrintLine("{0}", sndr.ID);
        }

        private void OnOnStateChange1(object sender, UInt16EventArgs args)
        {
            var sndr = (StateComponent)sender;
            CrestronConsole.PrintLine("1 Block:{2} MicIndex1:{0} MicIndex2:{3}State:{1}", sndr.ConfigInfo.Index1, args.Payload, sndr.ConfigInfo.InstanceTag, sndr.ConfigInfo.Index2);
            
        }

        private void OnOnStateChange2(object sender, UInt16EventArgs args)
        {
            var sndr = (StateComponent)sender;
            CrestronConsole.PrintLine("2 Block:{2} MicIndex1:{0} MicIndex2:{3}State:{1}", sndr.ConfigInfo.Index1, args.Payload, sndr.ConfigInfo.InstanceTag, sndr.ConfigInfo.Index2);
        }

        private void OnOnStateChangeAll(object sender, UInt16EventArgs args)
        {
            var sndr = (StateComponent)sender;
            CrestronConsole.PrintLine("All Block:{2} MicIndex1:{0} MicIndex2:{3}State:{1}", sndr.ConfigInfo.Index1, args.Payload, sndr.ConfigInfo.InstanceTag, sndr.ConfigInfo.Index2);
        }

        private void Poll()
        {
            biamp.SendHeartbeat();
            biamp.GetResponseTime();
        }

        private void Cmd(string cmdparameters)
        {
            if (cmdparameters.Length <= 0) return;

            var prms = cmdparameters.Split(' ');

            switch (prms[0])
            {
                case "connect":
                    biamp.Connect();
                    break;
                case ("poll"):
                    Poll();
                    break;
                case ("mute1"):
                    _mic1.SetState(ushort.Parse(prms[1]));
                    break;
                case ("mute2"):
                    _mic2.SetState(ushort.Parse(prms[1]));
                    break;
                case ("allmics"):
                    _allMics.SetState(ushort.Parse(prms[1]));
                    break;
                case ("preset"):
                    _presets.RecallPreset(prms[1]);
                    break;
                default:
                    CrestronConsole.PrintLine("Invalid args");
                    break;
            }
        }

        private void OnCommunicatingChange(object sender, UInt16EventArgs args)
        {
            CrestronConsole.PrintLine("Biamp Comms! args:{0}", args.Payload);
        }
    }
}

/*              case ("dial1"):
                    component1.CallSelect(1);
                    component1.Dial(prms[1]);
                    break;
                case ("help"):
                    component1.SelectSpeedDialEntry(1);
                    component1.DialSpeedDialEntry();
                    break;
                case ("dial2"):
                    component1.CallSelect(2);
                    component1.Dial(prms[1]);
                    break;
                case ("endcall1"):
                    component1.End_Call(1);
                    break;
                case ("endcall2"):
                    component1.End_Call(2);
                    break;
                case ("endall"):
                    component1.End_All();
                    break;
                case ("end"):
                    component1.End();
                    break;
                case ("on"):
                    component1.OnHook();
                    break;
                case ("off"):
                    component1.OffHook();
                    break;
 * 
 * component1 = new VoipDialerComponent();
                component1.Configure(1, "VoIP_Dialler", "VoIPControlStatus1", 1);
                component1.OnCallStatusListChange += ComponentOnOnCallStatusListChange;
                component1.OnInitializeChange += ComponentOnOnInitializeChange;

                component2 = new VoipDialerComponent();
                component2.Configure(1, "VoIP_Dialler", "VoIPControlStatus1", 2);
                component2.OnCallStatusListChange += ComponentOnOnCallStatusListChange;
 * 
 * 
 * private void ComponentOnOnCallStatusListChange2(object sender, VoipCallStatusesEventArgs args)
        {
            var status = new VoipCallStatus();
            var statusList = args.Payload;
            var sndr = (VoipDialerComponent)sender;

            statusList.Get(1, ref status);
            CrestronConsole.PrintLine("2 State.Number:{0}, status.ID:{1} Line:{2}", status.State.Number, status.ID, sndr.ConfigInfo.LineNumber);
        }

        private void ComponentOnOnInitializeChange(object sender, UInt16EventArgs args)
        {
            if (args.Payload == initState) return;
            CrestronConsole.PrintLine("ComponentOnOnInitializeChange:{0}", args.Payload);
            initState = args.Payload;
        }

        private void ComponentOnOnCallStatusListChange(object sender, VoipCallStatusesEventArgs args)
        {
            var sndr = (VoipDialerComponent)sender;
            var status = new VoipCallStatus();
            var statusList = args.Payload;

            statusList.Get(1, ref status);
            CrestronConsole.PrintLine("1 State.Number:{0}, status.ID:{1} Line:{2}", status.State.Number, status.ID, sndr.ConfigInfo.LineNumber);
        }
*/