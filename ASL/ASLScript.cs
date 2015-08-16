using LiveSplit.Model;
using System;
using System.Diagnostics;
using System.Dynamic;
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
        public ExpandoObject Vars { get; set; }
        public ASLMethod Init { get; set; }
        public ASLMethod Update { get; set; }
        public ASLMethod Start { get; set; }
        public ASLMethod Split { get; set; }
        public ASLMethod Reset { get; set; }
        public ASLMethod IsLoading { get; set; }
        public ASLMethod GameTime { get; set; }

        public ASLScript(
            string processName, ASLState state,
            ASLMethod init, ASLMethod update,
            ASLMethod start, ASLMethod reset,
            ASLMethod split, 
            ASLMethod isLoading, ASLMethod gameTime)
        {
            ProcessName = processName;
            State = state;
            Vars = new ExpandoObject();
            Init = init ?? new ASLMethod("");
            Update = update ?? new ASLMethod("");
            Start = start ?? new ASLMethod("");
            Split = split ?? new ASLMethod("");
            Reset = reset ?? new ASLMethod("");
            IsLoading = isLoading ?? new ASLMethod("");
            GameTime = gameTime ?? new ASLMethod("");
        }

        protected void TryConnect(LiveSplitState lsState)
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
                    Init.Run(lsState, OldState, State, Vars, Game);
                }
            }
        }

        public void RunUpdate(LiveSplitState lsState)
        {
            if (Game != null && !Game.HasExited)
            {
                OldState = State.RefreshValues(Game);

                Update.Run(lsState, OldState, State, Vars, Game);

                if (lsState.CurrentPhase == TimerPhase.Running || lsState.CurrentPhase == TimerPhase.Paused)
                {
                    var isPaused = IsLoading.Run(lsState, OldState, State, Vars, Game);
                    if (isPaused != null)
                        lsState.IsGameTimePaused = isPaused;

                    var gameTime = GameTime.Run(lsState, OldState, State, Vars, Game);
                    if (gameTime != null)
                        lsState.SetGameTime(gameTime);

                    if (Reset.Run(lsState, OldState, State, Vars, Game) ?? false)
                    {
                        Model.Reset();
                    }
                    else if (Split.Run(lsState, OldState, State, Vars, Game) ?? false)
                    {
                        Model.Split();
                    }
                }

                if (lsState.CurrentPhase == TimerPhase.NotRunning)
                {
                    if (Start.Run(lsState, OldState, State, Vars, Game) ?? false)
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
                TryConnect(lsState);
            }
        }
    }
}
