using System;
using System.Collections.Generic;
using NonsensicalKit.Core;
using NonsensicalKit.Core.Log;
using NonsensicalKit.Core.Service;

namespace NonsensicalKit.Tools.LogicNodeTreeSystem
{
    /// <summary>
    /// 此节点控制的判断条件
    /// </summary>
    public enum LogicNodeCheckType
    {
        SelfSelect, //自己是否被选中
        SelfUnselect, //自己是否未被选中
        ParentSelect, //自己或父节点是否被选中
        ParentUnselect, //自己或父节点是否未被选中
        ChildSelect, //自己或子节点是否被选中
        ChildUnselect, //自己或子节点是否未被选中
        ParentOrChildSelect, //自己或父节点或子节点被选中
        ParentOrChildUnselect //自己或父节点或子节点未被选中
    }

    public class LogicNodeManager : NonsensicalMono, IMonoService
    {
        public bool IsReady { get; set; }

        public Action InitCompleted { get; set; }

        public LogicNode CrtSelectNode { get; private set; } //当前选择的节点

        private LogicNode _root; //根节点

        private Dictionary<string, LogicNode> _dic = new Dictionary<string, LogicNode>(); //所有节点的字典，用于快速查找

        private string _switchBuffer; //记录未初始化前的切换，在初始化完成后执行

        #region Public Mothod

        public void InitConfig(LogicNodeTreeConfigData configData)
        {
            configData.OnAfterDeserialize();
            BuildLogicNodeTree(configData.Root);
            BuildDictionary();
            if (string.IsNullOrEmpty(_switchBuffer) == false)
            {
                SwitchNode(_switchBuffer);
            }

            CrtSelectNode = _root;
            if (!IsReady)
            {
                IsReady = true;
                InitCompleted?.Invoke();
                InitCompleted = null;
            }
            else
            {
                Publish((int)LogicNodeEnum.SwitchNode, CrtSelectNode);
            }
        }

        public LogicNode GetNode(string nodeID)
        {
            if (_dic.ContainsKey(nodeID))
            {
                return _dic[nodeID];
            }
            else
            {
                LogCore.Debug("未找到节点：" + nodeID);
                return null;
            }
        }

        public void SwitchNode(string nodeID)
        {
            if (_dic.ContainsKey(nodeID))
            {
                DoSwitchNode(_dic[nodeID]);
            }
            else
            {
                _switchBuffer = nodeID;
            }
        }

        /// <summary>
        /// 返回上一级
        /// </summary>
        public bool ReturnPreviousLevel()
        {
            if (CrtSelectNode != null)
            {
                if (CrtSelectNode.ParentNode == null)
                {
                    LogCore.Debug("当前为顶节点，无法返回上一级节点");
                    return false;
                }

                DoSwitchNode(CrtSelectNode.ParentNode);

                return true;
            }
            else
            {
                LogCore.Warning("当前未选择节点");

                return false;
            }
        }

        /// <summary>
        /// 检测所选节点是否被符合条件
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="checkType"></param>
        /// <returns></returns>
        public bool CheckState(string nodeID, LogicNodeCheckType checkType)
        {
            switch (checkType)
            {
                case LogicNodeCheckType.SelfSelect:
                    return CheckState(nodeID);
                case LogicNodeCheckType.SelfUnselect:
                    return !CheckState(nodeID);
                case LogicNodeCheckType.ParentSelect:
                    return CheckStateWithParent(nodeID);
                case LogicNodeCheckType.ParentUnselect:
                    return !CheckStateWithParent(nodeID);
                case LogicNodeCheckType.ChildSelect:
                    return CheckStateWithChild(nodeID);
                case LogicNodeCheckType.ChildUnselect:
                    return !CheckStateWithChild(nodeID);
                case LogicNodeCheckType.ParentOrChildSelect:
                    return CheckStateWithParent(nodeID) || CheckStateWithChild(nodeID, false);
                case LogicNodeCheckType.ParentOrChildUnselect:
                    return !CheckStateWithParent(nodeID) && !CheckStateWithChild(nodeID, false);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 检测所选节点是否被选中
        /// </summary>
        /// <param name="nodeID"></param>
        /// <returns></returns>
        public bool CheckState(string nodeID)
        {
            if (CrtSelectNode != null && _dic.ContainsKey(nodeID))
            {
                LogicNode crt = _dic[nodeID];

                if (crt == CrtSelectNode)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 检测所选节点父节点是否被选中
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="includeSelf"></param>
        /// <returns></returns>
        public bool CheckStateWithParent(string nodeID, bool includeSelf = true)
        {
            if (CrtSelectNode != null && _dic.ContainsKey(nodeID))
            {
                LogicNode crt;
                if (includeSelf)
                {
                    crt = _dic[nodeID];
                }
                else
                {
                    crt = _dic[nodeID].ParentNode;
                }

                while (crt != null)
                {
                    if (crt == CrtSelectNode)
                    {
                        return true;
                    }
                    else
                    {
                        crt = crt.ParentNode;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 检测所选节点子节点是否被选中
        /// </summary>
        /// <param name="nodeID"></param>
        /// <param name="includeSelf"></param>
        /// <returns></returns>
        public bool CheckStateWithChild(string nodeID, bool includeSelf = true)
        {
            if (CrtSelectNode != null && _dic.ContainsKey(nodeID))
            {
                LogicNode checkNode = _dic[nodeID];

                Queue<LogicNode> nodes = new Queue<LogicNode>();

                if (includeSelf)
                {
                    nodes.Enqueue(checkNode);
                }
                else
                {
                    foreach (var item in checkNode.ChildNode)
                    {
                        nodes.Enqueue(item);
                    }
                }

                while (nodes.Count > 0)
                {
                    LogicNode crt = nodes.Dequeue();
                    if (crt == CrtSelectNode)
                    {
                        return true;
                    }
                    else
                    {
                        foreach (var item in crt.ChildNode)
                        {
                            nodes.Enqueue(item);
                        }
                    }
                }
            }

            return false;
        }

        #endregion

        #region Private Method

        /// <summary>
        /// 构建节点树
        /// </summary>
        /// <param name="datas"></param>
        private void BuildLogicNodeTree(LogicNodeData root)
        {
            Queue<LogicNode> sns = new Queue<LogicNode>();
            Queue<LogicNodeData> snds = new Queue<LogicNodeData>();

            this._root = new LogicNode(root.NodeID, null, new LogicNode[root.Children.Count]);

            sns.Enqueue(this._root);
            snds.Enqueue(root);

            while (sns.Count > 0)
            {
                LogicNode crtNode = sns.Dequeue();
                LogicNodeData crtNodeData = snds.Dequeue();

                for (int i = 0; i < crtNodeData.Children.Count; i++)
                {
                    int length = crtNodeData.Children[i].Children.Count;
                    var newNode = new LogicNode(crtNodeData.Children[i].NodeID, crtNode, new LogicNode[length]);
                    crtNode.ChildNode[i] = newNode;
                    if (length > 0)
                    {
                        sns.Enqueue(newNode);
                        snds.Enqueue(crtNodeData.Children[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 构建字典
        /// </summary>
        private void BuildDictionary()
        {
            _dic = new Dictionary<string, LogicNode>();

            Queue<LogicNode> nodes = new Queue<LogicNode>();
            nodes.Enqueue(_root);

            while (nodes.Count > 0)
            {
                LogicNode crtSN = nodes.Dequeue();
                if (_dic.ContainsKey(crtSN.NodeID))
                {
                    LogCore.Warning($"节点名称重复:{crtSN.NodeID}");
                }
                else
                {
                    _dic.Add(crtSN.NodeID, crtSN);
                }

                foreach (var item in crtSN.ChildNode)
                {
                    nodes.Enqueue(item);
                }
            }
        }

        /// <summary>
        /// 正式执行节点切换
        /// </summary>
        /// <param name="targetNode"></param>
        private void DoSwitchNode(LogicNode targetNode)
        {
            if (string.IsNullOrEmpty(targetNode.AutoJump) == false)
            {
                targetNode = _dic[targetNode.AutoJump];
            }

            if (targetNode != CrtSelectNode)
            {
                LogCore.Debug("切换到节点:" + targetNode.NodeID);

                PublishWithID((int)LogicNodeEnum.NodeExit, CrtSelectNode.NodeID);

                CrtSelectNode = targetNode;

                PublishWithID((int)LogicNodeEnum.NodeEnter, targetNode.NodeID);

                Publish((int)LogicNodeEnum.SwitchNode, CrtSelectNode);
            }
        }

        #endregion
    }

    /// <summary>
    /// 树节点，存有父节点和子节点数组的信息
    /// </summary>
    public class LogicNode
    {
        public string NodeID;
        public string AutoJump; //自动跳转节点
        public LogicNode ParentNode;
        public LogicNode[] ChildNode;

        public LogicNode(string nodeID, LogicNode parentNode, LogicNode[] childNode)
        {
            NodeID = nodeID;
            ParentNode = parentNode;
            ChildNode = childNode;
        }
    }
}
