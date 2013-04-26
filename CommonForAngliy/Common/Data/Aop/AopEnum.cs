using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data.Aop
{
    /// <summary>
    /// ����ڲ����ݿ��������ö��
    /// </summary>
    public enum AopEnum
    {
        /// <summary>
        /// ��ѯ������¼����
        /// </summary>
        Select,
        /// <summary>
        /// ���뷽��
        /// </summary>
        Insert,
        /// <summary>
        /// ���·���
        /// </summary>
        Update,
        /// <summary>
        /// ɾ������
        /// </summary>
        Delete,
        /// <summary>
        /// ��ѯһ����¼����
        /// </summary>
        Fill,
        /// <summary>
        /// ȡ��¼����
        /// </summary>
        GetCount,
        /// <summary>
        /// MProc��ѯ����MDataTable����
        /// </summary>
        ExeMDataTable,
        /// <summary>
        /// MProcִ�з�����Ӱ����������
        /// </summary>
        ExeNonQuery,
        /// <summary>
        /// MProcִ�з����������з���
        /// </summary>
        ExeScalar
    }
}
