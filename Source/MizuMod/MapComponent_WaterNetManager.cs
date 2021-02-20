using System.Collections.Generic;
using Verse;

namespace MizuMod
{
    public class MapComponent_WaterNetManager : MapComponent
    {
        private bool requestedUpdateWaterNet;

        public MapComponent_WaterNetManager(Map map) : base(map)
        {
        }

        public List<WaterNet> Nets { get; } = new List<WaterNet>();

        public List<IBuilding_WaterNet> UnNetThings { get; } = new List<IBuilding_WaterNet>();

        public void RequestUpdateWaterNet()
        {
            requestedUpdateWaterNet = true;
        }

        private Queue<IBuilding_WaterNet> ClearWaterNets()
        {
            var unNetQueue = new Queue<IBuilding_WaterNet>();

            foreach (var t in UnNetThings)
            {
                t.InputWaterNet = null;
                t.OutputWaterNet = null;
                if (!unNetQueue.Contains(t))
                {
                    unNetQueue.Enqueue(t);
                }
            }

            UnNetThings.Clear();

            foreach (var net in Nets)
            {
                foreach (var t in net.AllThings)
                {
                    if (unNetQueue.Contains(t))
                    {
                        continue;
                    }

                    t.InputWaterNet = null;
                    t.OutputWaterNet = null;
                    unNetQueue.Enqueue(t);
                }

                net.ClearThings();
            }

            ClearNets();

            WaterNet.ClearNextID();

            return unNetQueue;
        }

        public void UpdateWaterNets()
        {
            var unNetQueue = ClearWaterNets();
            var unNetDiffQueue = new Queue<IBuilding_WaterNet>();

            while (unNetQueue.Count > 0)
            {
                var thing = unNetQueue.Dequeue();
                var inputNets = new List<WaterNet>();
                var outputNets = new List<WaterNet>();

                if (!thing.IsSameConnector)
                {
                    unNetDiffQueue.Enqueue(thing);
                    continue;
                }

                if (!thing.HasConnector)
                {
                    UnNetThings.Add(thing);
                    continue;
                }

                foreach (var net in Nets)
                {
                    foreach (var t in net.AllThings)
                    {
                        if (thing.IsOutputTo(t) && !outputNets.Contains(net))
                        {
                            outputNets.Add(t.InputWaterNet);
                        }

                        if (t.IsOutputTo(thing) && !inputNets.Contains(net))
                        {
                            inputNets.Add(t.OutputWaterNet);
                        }
                    }
                }

                var connectNets = new List<WaterNet>();
                connectNets.AddRange(inputNets);
                foreach (var net in outputNets)
                {
                    if (!connectNets.Contains(net))
                    {
                        connectNets.Add(net);
                    }
                }

                if (connectNets.Count == 0)
                {
                    // 0個=新しい水道網
                    var newNet = new WaterNet();
                    newNet.AddThing(thing);
                    AddNet(newNet);
                }
                else if (connectNets.Count == 1)
                {
                    // 1個=既存の水道網に加える
                    if (!connectNets[0].AllThings.Contains(thing))
                    {
                        connectNets[0].AddThing(thing);
                    }
                }
                else
                {
                    // 2個以上=新しい物と、既存の水道網を全て最初の水道網に結合する
                    if (!connectNets[0].AllThings.Contains(thing))
                    {
                        connectNets[0].AddThing(thing);
                    }

                    for (var i = 1; i < connectNets.Count; i++)
                    {
                        // 消滅する水道網に所属している物を全て移し替える
                        foreach (var t in connectNets[i].AllThings)
                        {
                            if (!connectNets[0].AllThings.Contains(t))
                            {
                                connectNets[0].AddThing(t);
                            }
                        }

                        // 接続水道網の終えたので水道網を削除
                        Nets.Remove(connectNets[i]);
                    }
                }
            }

            while (unNetDiffQueue.Count > 0)
            {
                var thing = unNetDiffQueue.Dequeue();
                var inputNets = new List<WaterNet>();
                var outputNets = new List<WaterNet>();

                if (!thing.HasConnector)
                {
                    UnNetThings.Add(thing);
                    continue;
                }

                foreach (var net in Nets)
                {
                    foreach (var t in net.AllThings)
                    {
                        if (thing.IsOutputTo(t) && !outputNets.Contains(net))
                        {
                            outputNets.Add(t.InputWaterNet);
                        }

                        if (t.IsOutputTo(thing) && !inputNets.Contains(net))
                        {
                            inputNets.Add(t.OutputWaterNet);
                        }
                    }
                }

                if (inputNets.Count == 0)
                {
                    // 0個=新しい水道網
                    var newNet = new WaterNet();
                    newNet.AddInputThing(thing);
                    AddNet(newNet);
                }
                else if (inputNets.Count == 1)
                {
                    // 1個=既存の水道網に加える
                    if (!inputNets[0].AllThings.Contains(thing))
                    {
                        inputNets[0].AddInputThing(thing);
                    }
                }
                else
                {
                    // 2個以上=新しい物と、既存の水道網を全て最初の水道網に結合する
                    if (!inputNets[0].AllThings.Contains(thing))
                    {
                        inputNets[0].AddInputThing(thing);
                    }

                    for (var i = 1; i < inputNets.Count; i++)
                    {
                        // 消滅する水道網に所属している物を全て移し替える
                        foreach (var t in inputNets[i].AllThings)
                        {
                            if (!inputNets[0].AllThings.Contains(t))
                            {
                                inputNets[0].AddInputThing(t);
                            }
                        }

                        // 接続水道網の終えたので水道網を削除
                        Nets.Remove(inputNets[i]);
                    }
                }

                if (outputNets.Count == 0)
                {
                    // 0個=新しい水道網
                    var newNet = new WaterNet();
                    newNet.AddOutputThing(thing);
                    AddNet(newNet);
                }
                else if (outputNets.Count == 1)
                {
                    // 1個=既存の水道網に加える
                    if (!outputNets[0].AllThings.Contains(thing))
                    {
                        outputNets[0].AddOutputThing(thing);
                    }
                }
                else
                {
                    // 2個以上=新しい物と、既存の水道網を全て最初の水道網に結合する
                    if (!outputNets[0].AllThings.Contains(thing))
                    {
                        outputNets[0].AddOutputThing(thing);
                    }

                    for (var i = 1; i < outputNets.Count; i++)
                    {
                        // 消滅する水道網に所属している物を全て移し替える
                        foreach (var t in outputNets[i].AllThings)
                        {
                            if (!outputNets[0].AllThings.Contains(t))
                            {
                                outputNets[0].AddOutputThing(t);
                            }
                        }

                        // 接続水道網の終えたので水道網を削除
                        Nets.Remove(outputNets[i]);
                    }
                }
            }
        }

        public void AddThing(IBuilding_WaterNet thing)
        {
            UnNetThings.Add(thing);
            UpdateWaterNets();
        }

        public void RemoveThing(IBuilding_WaterNet thing)
        {
            // 対象の物を除去
            thing.InputWaterNet?.RemoveThing(thing);

            thing.OutputWaterNet?.RemoveThing(thing);

            if (UnNetThings.Contains(thing))
            {
                UnNetThings.Remove(thing);
            }

            UpdateWaterNets();
        }

        private void AddNet(WaterNet net)
        {
            net.Manager = this;
            Nets.Add(net);
        }

        public void RemoveNet(WaterNet net)
        {
            net.Manager = null;
            Nets.Remove(net);
        }

        private void ClearNets()
        {
            foreach (var net in Nets)
            {
                net.Manager = null;
            }

            Nets.Clear();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (requestedUpdateWaterNet)
            {
                requestedUpdateWaterNet = false;
                UpdateWaterNets();
            }

            // 入力量と入力水質、水道網全体の水質を更新
            foreach (var net in Nets)
            {
                net.UpdateInputWaterFlow();
            }

            // タンクの水量とタンク内の水質を更新
            foreach (var net in Nets)
            {
                net.UpdateWaterTank();
            }
        }
    }
}