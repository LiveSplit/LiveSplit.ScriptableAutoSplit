using LiveSplit.Model;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.ASL
{
    public class ASLScript
    {
        protected TimerModel Model { get; set; }
        protected Process Game { get; set; }
        public String ProcessName { get; set; }
        public ASLState OldState { get; set; }
        public ASLState State { get; set; }
        public ASLMethod Start { get; set; }
        public ASLMethod Split { get; set; }
        public ASLMethod Reset { get; set; }
        public ASLMethod IsLoading { get; set; }
        public ASLMethod GameTime { get; set; }

        public ASLScript(
            String processName, ASLState state, 
            ASLMethod start, ASLMethod split, 
            ASLMethod reset, 
            ASLMethod isLoading, ASLMethod gameTime)
        {
            ProcessName = processName;
            State = state;
            Start = start ?? new ASLMethod("");
            Split = split ?? new ASLMethod("");
            Reset = reset ?? new ASLMethod("");
            IsLoading = isLoading ?? new ASLMethod("");
            GameTime = gameTime ?? new ASLMethod("");
        }

        protected void TryConnect()
        {
            if (Game == null || Game.HasExited)
            {
                Game = null;
                var process = Process.GetProcessesByName(ProcessName).FirstOrDefault();
                if (process != null)
                {
                    Game = process;
                    State.RefreshValues(Game);
                    OldState = State;
                }
            }
        }

        public void Update(LiveSplitState lsState)
        {
            if (Game != null && !Game.HasExited)
            {
                OldState = State.RefreshValues(Game);

                if (lsState.CurrentPhase == TimerPhase.Running || lsState.CurrentPhase == TimerPhase.Paused)
                {
                    var isPaused = IsLoading.Run(lsState, OldState, State);
                    if (isPaused != null)
                        lsState.IsGameTimePaused = isPaused;

                    var gameTime = GameTime.Run(lsState, OldState, State);
                    if (gameTime != null)
                        lsState.SetGameTime(gameTime);

                    if (Reset.Run(lsState, OldState, State) ?? false)
                    {
                        Model.Reset();
                    }
                    else if (Split.Run(lsState, OldState, State) ?? false)
                    {
                        Model.Split();
                    }
                }

                if (lsState.CurrentPhase == TimerPhase.NotRunning)
                {
                    if (Start.Run(lsState, OldState, State) ?? false)
                    {
                        Model.Start();
                    }
                }
            }
            else
            {
                if (Model == null)
                {
                    Model = new TimerModel() { CurrentState = lsState };
                }
                TryConnect();
            }
        }
    }
}
