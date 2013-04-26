using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data.Aop
{
    /// <summary>
    /// Aop�ӿڣ���Ҫʵ��ʱ�̳�
    /// </summary>
    public interface IAop
    {
        /// <summary>
        /// ��������֮ǰ������
        /// </summary>
        /// <param name="action">��������</param>
        /// <param name="objName">����/�洢������/��ͼ��/sql���</param>
        /// <param name="aopInfo">������֧����</param>
        void Begin(AopEnum action,string objName, params object[] aopInfo);
        /// <summary>
        /// ��������֮�󱻵���
        /// </summary>
        /// <param name="action">��������</param>
        /// <param name="success">�����Ƿ�ɹ�</param>
        /// <param name="id">һ����ú��id[������ֵ]</param>
        /// <param name="aopInfo">������֧����</param>
        void End(AopEnum action, bool success, object id, params object[] aopInfo);
        /// <summary>
        /// ���ݿ���������쳣ʱ,�����˷���
        /// </summary>
        /// <param name="msg"></param>
        void OnError(string msg);
        /// <summary>
        /// �ڲ���ȡ����Aop���ⲿʹ�÷���null���ɡ�
        /// </summary>
        /// <returns></returns>
        IAop GetFromConfig();
    }
}
