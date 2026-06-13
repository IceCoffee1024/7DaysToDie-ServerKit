//using ConcurrentCollections;
//using GamePath;
//using System;
//using System.Collections;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace SdtdServerKit.HarmonyPatchers.PerformanceTuning
//{
//    public class ASPPathFinderThread : PathFinderThread
//    {
//        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

//        private readonly ConcurrentHashSet<int> entityWaitQueue = new ConcurrentHashSet<int>();

//        private readonly ConcurrentDictionary<int, PathInfo> finishedPaths = new ConcurrentDictionary<int, PathInfo>();

//        public ASPPathFinderThread()
//        {
//            Instance = this;
//        }

//        public override int GetFinishedCount()
//        {
//            return finishedPaths.Count;
//        }

//        public override int GetQueueCount()
//        {
//            return entityWaitQueue.Count;
//        }

//        public override void StartWorkerThreads()
//        {
//            Task.Factory.StartNew(FindPaths, tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
//        }

//        public override void Cleanup()
//        {
//            entityWaitQueue.Clear();
//            finishedPaths.Clear();
//            tokenSource.Cancel();
//        }

//        private void FindPaths()
//        {
//            while (tokenSource.IsCancellationRequested == false)
//            {
//                while (entityWaitQueue.TryRemoveFirst(out int entityId))
//                {
//                    if (finishedPaths.TryGetValue(entityId, out var pathInfo))
//                    {
//                        Task.Run(() =>
//                        {
//                            try
//                            {
//                                pathInfo.entity.navigator.GetPathTo(pathInfo);
//                                if (pathInfo.state == PathInfo.State.Queued)
//                                {
//                                    finishedPaths.TryRemove(entityId, out var _);
//                                }
//                            }
//                            catch (Exception ex)
//                            {
//                                CustomLogger.Warn(ex, "{0} path dup id {1}", GameManager.frameCount, entityId);
//                            }
//                        });
//                    }
//                }

//                // The operating system processes or threads length of the time slice
//                Thread.Sleep(20);
//            }
//        }

//        public override void FindPath(EntityAlive _entity, Vector3 _targetPos, float _speed, bool _canBreak, EAIBase _aiTask)
//        {
//            entityWaitQueue.Add(_entity.entityId);
//            finishedPaths[_entity.entityId] = new PathInfo(_entity, _targetPos, _canBreak, _speed, _aiTask);
//        }

//        public override void FindPath(EntityAlive _entity, Vector3 _startPos, Vector3 _targetPos, float _speed, bool _canBreak, EAIBase _aiTask)
//        {
//            entityWaitQueue.Add(_entity.entityId);
//            PathInfo pathInfo = new PathInfo(_entity, _targetPos, _canBreak, _speed, _aiTask);
//            pathInfo.SetStartPos(_startPos);
//            finishedPaths[_entity.entityId] = pathInfo;
//        }

//        public override PathInfo GetPath(int _entityId)
//        {
//            if (finishedPaths.TryGetValue(_entityId, out var pathInfo) && pathInfo.state == PathInfo.State.Done)
//            {
//                finishedPaths.TryRemove(_entityId, out var _);
//                return pathInfo;
//            }

//            return PathInfo.Empty;
//        }

//        public override bool IsCalculatingPath(int _entityId)
//        {
//            return finishedPaths.ContainsKey(_entityId);
//        }

//        public override void RemovePathsFor(int _entityId)
//        {
//            finishedPaths.TryRemove(_entityId, out var _);
//            entityWaitQueue.TryRemove(_entityId);
//        }
//    }
//}
