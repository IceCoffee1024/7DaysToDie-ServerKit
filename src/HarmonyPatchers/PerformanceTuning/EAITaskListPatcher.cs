//using HarmonyLib;

//namespace SdtdServerKit.HarmonyPatchers.PerformanceTuning
//{
//    [HarmonyPatch(typeof(EAITaskList))]
//    internal class EAITaskListPatcher
//    {
//        [HarmonyPrefix]
//        [HarmonyPatch(nameof(EAITaskList.OnUpdateTasks))]
//        public static bool OnUpdateTasks(
//            List<EAITaskEntry> ___allTasks,
//            List<EAITaskEntry> ___executingTasks,
//            //List<EAITaskEntry> ___startedTasks,
//            float ___executeDelayScale)
//        {
//            //___startedTasks.Clear();

//            //CustomLogger.Warn("___allTasks count:" + ___allTasks.Count);
//            Parallel.ForEach(___allTasks, (eAITaskEntry) =>
//            {
//                try
//                {
//                    if (eAITaskEntry.isExecuting)
//                    {
//                        if (eAITaskEntry.action.Continue() && isBestTask(eAITaskEntry, ___executingTasks))
//                        {
//                            return;
//                        }

//                        lock (___executingTasks)
//                        {
//                            ___executingTasks.Remove(eAITaskEntry);
//                        }

//                        eAITaskEntry.isExecuting = false;
//                        eAITaskEntry.executeTime = eAITaskEntry.action.executeDelay * ___executeDelayScale;
//                        eAITaskEntry.action.Reset();
//                    }

//                    eAITaskEntry.executeTime -= 0.05f;
//                    eAITaskEntry.action.executeWaitTime += 0.05f;
//                    if (eAITaskEntry.executeTime > 0f)
//                    {
//                        return;
//                    }

//                    eAITaskEntry.executeTime = eAITaskEntry.action.executeDelay * ___executeDelayScale;
//                    if (isBestTask(eAITaskEntry, ___executingTasks))
//                    {
//                        if (eAITaskEntry.action.CanExecute())
//                        {
//                            //lock (___startedTasks)
//                            //{
//                            //    ___startedTasks.Add(eAITaskEntry);
//                            //}

//                            eAITaskEntry.isExecuting = true;
//                            eAITaskEntry.action.Start();

//                            lock (___executingTasks)
//                            {
//                                ___executingTasks.Add(eAITaskEntry);
//                            }
//                        }

//                        eAITaskEntry.action.executeWaitTime = 0f;
//                    }
//                }
//                catch (Exception ex)
//                {
//                    CustomLogger.Error(ex, "Error in EAITaskListPatcher.OnUpdateTasks~Parallel.ForEach~0");
//                }
//            });

//            //CustomLogger.Warn("___startedTasks count:" + ___startedTasks.Count);
//            //Parallel.ForEach(___startedTasks, (eAITaskEntry) =>
//            //{
//            //    try
//            //    {
//            //        eAITaskEntry.action.Start();
//            //    }
//            //    catch (Exception ex)
//            //    {
//            //        CustomLogger.Error(ex, "Error in EAITaskListPatcher.OnUpdateTasks~Parallel.ForEach~1");
//            //    }
//            //});

//            //CustomLogger.Warn("___executingTasks count:" + ___executingTasks.Count);

//            for (int k = 0; k < ___executingTasks.Count; k++)
//            {
//                ___executingTasks[k].action.Update();
//            }

//            return false;
//        }

//        private static bool isBestTask(EAITaskEntry _task, List<EAITaskEntry> executingTasks)
//        {
//            int i = 0;
//            lock (executingTasks)
//            {
//                while (i < executingTasks.Count)
//                {
//                    EAITaskEntry eaitaskEntry = executingTasks[i++];
//                    if (eaitaskEntry != _task)
//                    {
//                        if (eaitaskEntry.priority > _task.priority)
//                        {
//                            if (eaitaskEntry.action.IsContinuous())
//                            {
//                                continue;
//                            }
//                        }
//                        else if (areTasksCompatible(_task, eaitaskEntry))
//                        {
//                            continue;
//                        }
//                        return false;
//                    }
//                }
//            }
            
//            return true;
//        }

//        private static bool areTasksCompatible(EAITaskEntry _task, EAITaskEntry _other)
//        {
//            return (_task.action.MutexBits & _other.action.MutexBits) == 0;
//        }
//    }
//}
