/**********************************************文档说明**************************************************
 * 文件名称:    IEProcedure.cs 树搜索
 * 作者:        Nene Sakura 
 * 版本:        0.1.0        
 * 创建日期:    2018.10.21, 17:45 
 * 完成日期:    NULL
 * 文件描述:   双层混合整数优化 的树搜索隐枚举算法 过程文件
 *              
 * 调用关系:   被相关问题的模型文件调用 
 * 继承关系:    无
 * 其它:        无
 * 属性列表:    略
 * 函数列表:    
 * 
 * 修改历史:
 * 1.  修改日期: 无
 *      修改人:   无
 *      修改功能: 无
 * 
*********************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ControlCenter.Assignment.ReconnaissanceAndSearch
{
    /// <summary>
    /// IE树搜索类
    /// </summary>
    public class TreeSearchProcedure<T>
    {
        /// <summary>
        ///优化过程是否是求最小是求最小值
        /// </summary>
        public bool m_IsMaxmize = false;
        /// <summary>
        /// 获取及设置优化过程是否是求最小是求最小值 
        /// </summary>
        public bool IsMaxmize
        {
            get { return m_IsMaxmize; }
            set { m_IsMaxmize = value; }
        }

        /// <summary>
        /// 从候选集中选择一个元素(委托)
        /// </summary>
        /// <param name="mCandidates">候选集</param>
        /// <returns>被选中的元素</returns>
        public delegate T CandidateSelection(List<T> mCandidates);
        /// <summary>
        /// 选择函数
        /// </summary>
        public CandidateSelection SelectFromCandidateSet;

        /// <summary>
        /// 求解一个结点，求解这个结点下下层的最优解(委托)
        /// </summary>
        /// <param name="mNode">结点对象</param>
        /// <returns>策略</returns>
        public delegate List<T> NodeSolution(TreeSearchNode<T> mNode);
        /// <summary>
        ///  求解一个结点，求解这个结点下下层的最优解(委托)
        ///  求解这个节点通常要满足三两个步骤
        /// 1. 更新目标函数值Preference 2. 更新CandidateSet 
        /// 3. 更新并返回下层最优策略（可根据情况只更新不反悔）
        /// </summary>
        public NodeSolution SolvingANode;

        /// <summary>
        /// 上层资源的总量 (尽量不要更改)
        /// </summary>
        public int TotalResource = 0;

        /// <summary>
        /// 用来存储整个枚举树
        /// </summary>
        private List<TreeSearchNode<T>> m_SearchTree;

        /// <summary>
        /// 上层的最优策略
        /// </summary>
        private List<T> m_OptimalStrategy;
        /// <summary>
        /// 获取或设置上层最优策略
        /// </summary>
        public List<T> OptimalStrategy
        {
            get { return m_OptimalStrategy; }
            set { m_OptimalStrategy = value; }
        }

        /// <summary>
        /// 最优值
        /// </summary>
        private double m_OptimalValue;
        /// <summary>
        /// 获取或设置最优值
        /// </summary>
        public double OptimalValue
        {
            get { return m_OptimalValue; }
            set { m_OptimalValue = value; }
        }


        
        /// <summary>
        /// 整个枚举树搜索过程
        /// 这里忽略下一级的求解细节（具体用委托实现）
        /// </summary>
        public void TreeSearchProcess()
        {
            #region 设置根节点
            //根结o
            TreeSearchNode<T> mRootNode = new TreeSearchNode<T>();
            mRootNode.Preference = 0;
            mRootNode.Fix0Set = new List<T>();     // 根节点上层Fix0的集合
            mRootNode.Fix1Set = new List<T>();     //  根节点上层Fix1的集合
            mRootNode.CandidateSet = new List<T>();    // 初始化根节点
            mRootNode.Index = 1;    // 根节点编号
            mRootNode.FatherIndex = -1;    // 根节点的前驱节点
            SolvingANode(mRootNode);    // 求解的一个节点
            mRootNode.Index = 0;    // 根节点编号（我也不知道当时为什么要遍两遍）
            m_OptimalStrategy = mRootNode.Fix1Set;     // 初始化最优解
            m_OptimalValue = mRootNode.Preference;    // 初始化最优值
            m_SearchTree = new List<TreeSearchNode<T>>();    // 初始化搜索树（一个空树）
            m_SearchTree.Add(mRootNode);    // 
            #endregion

            #region 树搜索结点展开及分支过程
            for (int dNodePointer = 0; dNodePointer < m_SearchTree.Count; dNodePointer++ )    // dNodePointer:节点编号
            {
                #region 求解当前结点
                // 求解当前结点
                SolvingANode(m_SearchTree[dNodePointer]);    //求解当前的节点（初始版本没有保留 下层解的值，计划给加上）
                #endregion

                #region 判断/更新最优
                // 判断/更新最优 
                if (!m_IsMaxmize)    // 最小值优化
                {
                    if (m_SearchTree[dNodePointer].Preference < m_OptimalValue)     // 判断/更新当前的最优解
                    {
                        m_OptimalValue = m_SearchTree[dNodePointer].Preference;     
                        m_OptimalStrategy = m_SearchTree[dNodePointer].Fix1Set;                        
                    }
                }
                else    // 最大值优化
                {
                    if (m_SearchTree[dNodePointer].Preference > m_OptimalValue)
                    {
                        m_OptimalValue = m_SearchTree[dNodePointer].Preference;
                        m_OptimalStrategy = m_SearchTree[dNodePointer].Fix1Set;
                    }
                }
                #endregion

                #region 扩展新的孩子结点
                //扩展新的孩子结点
                //若候选集为空则继续，即该节点没有候选集，或是资源不允许在往下扩张
                if (m_SearchTree[dNodePointer].CandidateSet == null || m_SearchTree[dNodePointer].CandidateSet.Count == 0
                    || TotalResource == m_SearchTree[dNodePointer].Fix1Set.Count)
                {
                    continue;
                }
                //从候选集选择一个策略元素,
                T mTmpElement = SelectFromCandidateSet(m_SearchTree[dNodePointer].CandidateSet);
                #region 扩展左孩子结点
                //扩展左孩子结点
                TreeSearchNode<T> mLeftNode = new TreeSearchNode<T>();    // 初始化左边 的孩子结点
                mLeftNode.Index = m_SearchTree.Count;    // 节点编号
                mLeftNode.FatherIndex = dNodePointer;    // 设置节点的父节点
                // 因为还没有求解，所以先将左孩子节点 继承其父节点的最优解 （为了将来比较用）
                mLeftNode.Preference = m_SearchTree[dNodePointer].Preference;    // 搜索树的目标函数值(叫ObjectiveValue比较好)， 这里懒得改了
                mLeftNode.Fix0Set = new List<T>();    //重写Fix0，这里需重新赋值，不能传递引用 
                for (int j = 0; j < (m_SearchTree[dNodePointer].Fix0Set != null ? m_SearchTree[dNodePointer].Fix0Set.Count : 0); j++)
                {
                    mLeftNode.Fix0Set.Add(m_SearchTree[dNodePointer].Fix0Set[j]); 
                }
                mLeftNode.Fix1Set = new List<T>();
                for (int j = 0; j < (m_SearchTree[dNodePointer].Fix1Set != null ? m_SearchTree[dNodePointer].Fix1Set.Count : 0); j++)
                {
                    mLeftNode.Fix1Set.Add(m_SearchTree[dNodePointer].Fix1Set[j]);
                }
                mLeftNode.Fix1Set.Add(mTmpElement);    // 将新选中的策略元素置1
                mLeftNode.CandidateSet = new List<T>();  // 初始化左孩子节点的候选集
                for (int j = 0; j < (m_SearchTree[dNodePointer].CandidateSet != null ? m_SearchTree[dNodePointer].CandidateSet.Count : 0); j++)
                {
                    mLeftNode.CandidateSet.Add(m_SearchTree[dNodePointer].CandidateSet[j]);     // 这步不知道有什么用，因为多fix一个1后，整个候选集可能刷新
                }   
                m_SearchTree.Add(mLeftNode);
        
                #endregion

                #region 扩展右孩子结点
                //扩展有孩子节点
                TreeSearchNode<T> mRightNode = new TreeSearchNode<T>();
                //mRightNode.Index = m_SearchTree.Count + 1;  //这步没明白，为什么要加1呢？加了左节点后应该已经Count应该已经变化了啊
                mRightNode.Index = m_SearchTree.Count;  //这步没明白，为什么要加1呢？加了左节点后应该已经Count应该已经变化了啊
                mRightNode.FatherIndex = dNodePointer;
                mRightNode.Preference = m_SearchTree[dNodePointer].Preference;
                mRightNode.Fix0Set = new List<T>();
                for (int j = 0; j < (m_SearchTree[dNodePointer].Fix0Set != null ? m_SearchTree[dNodePointer].Fix0Set.Count : 0); j++)
                {
                    mRightNode.Fix0Set.Add(m_SearchTree[dNodePointer].Fix0Set[j]);
                }
                mRightNode.Fix0Set.Add(mTmpElement);
                mRightNode.Fix1Set = new List<T>();
                for (int j = 0; j < (m_SearchTree[dNodePointer].Fix1Set != null ? m_SearchTree[dNodePointer].Fix1Set.Count : 0); j++)
                {
                    mRightNode.Fix1Set.Add(m_SearchTree[dNodePointer].Fix1Set[j]);
                }                
                mRightNode.CandidateSet = new List<T>();
                for (int j = 0; j < (m_SearchTree[dNodePointer].CandidateSet != null ? m_SearchTree[dNodePointer].CandidateSet.Count : 0); j++)
                {
                    mRightNode.CandidateSet.Add(m_SearchTree[dNodePointer].CandidateSet[j]);
                }
                m_SearchTree.Add(mRightNode);
                #endregion

                #endregion
            }
            #endregion

            //System.Windows.Forms.MessageBox.Show(m_SearchTree.Count.ToString());
        }
    }

    /// <summary>
    /// IE方法的一个结点
    /// </summary>
    public class TreeSearchNode<T> //: IComparable<TreeSearchNode<T>>
    {
        /// <summary>
        /// 结点编号
        /// </summary>
        public int Index;
        /// <summary>
        /// 父节点编号
        /// </summary>
        public int FatherIndex;
        /// <summary>
        /// 候选集
        /// </summary>
        private List<T> m_CandidateSet;
        /// <summary>
        /// 获取或设置候选集
        /// </summary>
        public List<T> CandidateSet
        {
            get { return m_CandidateSet; }
            set { m_CandidateSet = value; }
        }
        /// <summary>
        /// Fix为1的集合
        /// </summary>
        private List<T> m_Fix1Set;
        /// <summary>
        /// 获取或设置Fix为1的集合
        /// </summary>
        public List<T> Fix1Set
        {
            get { return m_Fix1Set; }
            set { m_Fix1Set = value; }
        }

        /// <summary>
        /// Fix为0的集合
        /// </summary>
        private List<T> m_Fix0Set;
        /// <summary>
        /// 获取或设置Fix为0的集合
        /// </summary>
        public List<T> Fix0Set
        {
            get { return m_Fix0Set; }
            set { m_Fix0Set = value; }
        }

        /// <summary>
        /// 结点偏好值
        /// </summary>
        private double m_Preference = 0;
        /// <summary>
        /// 获取或设置结点偏好值
        /// </summary>
        public double Preference
        {
            get { return m_Preference; }
            set { m_Preference = value; }
        }

        /// <summary>
        /// 该节点约束下（即Fix1)下层的最优策略 
        /// </summary>
        private List<T> m_LowerLevelStrategy;
        /// <summary>
        /// 获取或设置该节点约束下（即Fix1)下层的最优策略 
        /// </summary>
        public List<T> LowerLevelStrategy
        {
            get { return m_LowerLevelStrategy; }
            set { m_LowerLevelStrategy = value; }
        }
/*
        public int CompareTo(TreeSearchNode<T> other)
        {
            if (other == null)
            {
                return 1;
            }
            if(this.m_Preference == other.Preference)
            {
                return 0;
            }
            if (this.m_Preference > other.Preference)
            {
                return 1;
            }
            return -1;
        }

        public bool Equals(TreeSearchNode<T> other)
        {
            return (this.CompareTo(other) == 0);
        }
*/
    }    

    

    

    
}
