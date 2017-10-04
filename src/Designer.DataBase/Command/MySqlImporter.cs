using System;
using System.Collections;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using Agebull.CodeRefactor.SolutionManager;
using Agebull.Common.SimpleDesign;
using Gboxt.Common.DataAccess.Schemas;
using Gboxt.Common.WpfMvvmBase.Commands;
using WpfMvvmBase.Coefficient;

namespace Agebull.Common.Config.Designer
{
    /// <summary>
    /// ��ϵ���Ӽ��
    /// </summary>
    [Export(typeof(IAutoRegister))]
    [ExportMetadata("Symbol", '%')]
    internal sealed class MySqlImporter : ConfigCommandBase<SolutionConfig>, IAutoRegister
    {
        /// <summary>
        /// ע�����
        /// </summary>
        void IAutoRegister.AutoRegist()
        {
            NoButton = true;
            Signle = true;
            CommandCoefficient.RegisterCommand<SolutionConfig, MySqlImporter>();
        }


        public override CommandItem ToCommand(object arg, Func<object, IEnumerator> enumerator = null)
        {
            return new CommandItem
            {
                NoButton = true,
                Signle = true,
                SourceType = typeof(SolutionConfig).Name,
                Command = new AsyncCommand<string, string>(SyncMySqlStructParpare, SyncMySqlStruct, SyncMySqlStructEnd),
                Name = "����MySql���ݿ�",
                Image = Application.Current.Resources["tree_Assembly"] as ImageSource
            };
        }

        public bool SyncMySqlStructParpare(string arg, Action<string> setAction)
        {
            var ctx = DataModelDesignModel.Current.Context;
            ctx.NowJob = DesignContext.JobTrace;
            return MessageBox.Show("ȷ��ִ��ͬ�����ݿ�ṹ������?", "����༭", MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        }


        /// <summary>
        ///     ����MySql���ݿ�
        /// </summary>
        /// <returns></returns>
        internal string SyncMySqlStruct(string arg)
        {
            var ctx = DataModelDesignModel.Current.Context;
            new MySqlImport().Import(ctx.CurrentTrace.TraceMessage, ctx.Solution, DataModelDesignModel.Current.Dispatcher);
            return string.Empty;
        }

        internal void SyncMySqlStructEnd(CommandStatus status, Exception ex, string code)
        {
            DataModelDesignModel.Current.Context.NowJob = DesignContext.JobTrace;
            if (ex != null)
                DataModelDesignModel.Current.Context.CurrentTrace.TraceMessage.Track = ex.ToString();
        }
    }
}