﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agebull.Common;

namespace Gboxt.Common.DataAccess.Schemas
{
    /// <summary>
    /// 实体配置业务模型
    /// </summary>
    internal class EntityBusinessModel : ConfigModelBase
    {
        /// <summary>
        /// 表结构对象
        /// </summary>
        public EntityConfig Entity { get; set; }


        #region RepairByModel

        /// <summary>
        ///     自动修复(从模型修复数据存储)
        /// </summary>
        public void RepairByModel(bool isReference = false)
        {
            if (Entity.IsFreeze)
                return;
            if (Entity.IsReference)
            {
                foreach (var col in Entity.Properties)
                {
                    col.ColumnName = null;
                    col.DbType = null;
                }
                Entity.IsModify = true;
                Entity.IsClass = true;
                Entity.ReadTableName = null;
                Entity.SaveTableName = null;
                return;
            }
            if (Entity.IsFreeze || Entity.IsReference)
                return;
            RepairCaption();
            if (Entity.IsClass)
            {
                Entity.ReadTableName = null;
                Entity.SaveTableName = null;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Entity.ReadTableName))
                    Entity.ReadTableName = ToWords(Entity.Name).LinkToString("tb_", "_");
                if (Entity.PrimaryColumn == null)
                {
                    Entity.Properties.Add(new PropertyConfig
                    {
                        Name = Entity.Name + "Id",
                        Caption = Entity.Caption + "ID",
                        Description = Entity.Caption + "ID",
                        IsPrimaryKey = true,
                        IsIdentity = true,
                        CsType = "int",
                        CppType = "int",
                        Parent = Entity
                    });
                }
            }
            EntityConfig friend = GetEntity(p => p != Entity && p.Tag == Entity.Tag);

            PropertyBusinessModel model = new PropertyBusinessModel();
            foreach (var col in Entity.Properties)
            {
                if (col.Discard || col.IsFreeze)
                {
                    continue;
                }
                col.Parent = Entity;
                model.Property = col;
                model.RepairByModel(isReference, friend);
                col.IsModify = true;
            }
            Entity.IsModify = true;
        }

        
        
        private void RepairCaption()
        {
            RepairConfigName(Entity);

            if (!string.IsNullOrWhiteSpace(Entity.Description))
            {
                Entity.Description = Entity.Description.Trim(NoneLanguageChar);
            }
            if (!string.IsNullOrWhiteSpace(Entity.Caption))
            {
                Entity.Caption = Entity.Caption.Trim(NoneLanguageChar);
            }
            if (string.IsNullOrWhiteSpace(Entity.Caption))
            {
                Entity.Caption = !string.IsNullOrWhiteSpace(Entity.Description) ? Entity.Description : Entity.Name;
            }
            if (string.IsNullOrWhiteSpace(Entity.Description))
            {
                Entity.Description = Entity.Caption;
            }
        }
        #endregion
        #region 修复
        /// <summary>
        ///     中译英
        /// </summary>
        public void ToEnglish()
        {
            if (Entity.IsFreeze)
                return;
            foreach (var col in Entity.Properties)
            {
                if (string.IsNullOrEmpty(col.Caption) || (!string.IsNullOrWhiteSpace(col.Name) && col.Name[0] < 256))
                    continue;
                col.Name = BaiduFanYi.FanYiWord(col.Caption);
            }
            Entity.IsModify = true;
        }
        /// <summary>
        ///     自动修复(从模型修复数据存储)
        /// </summary>
        public void RepairRegular(bool repair)
        {
            if (Entity.IsFreeze)
                return;
            foreach (var col in Entity.Properties)
            {
                if (col.Discard)
                {
                    continue;
                }
                if (col.IsPrimaryKey)
                {
                    col.Nullable = false;
                    col.DbNullable = false;
                    col.KeepStorageScreen = StorageScreenType.Update;
                }
                if (col.IsIdentity || col.IsSystemField)
                    col.IsUserReadOnly = true;
                if (col.Nullable)
                    col.DbNullable = true;
                if (col.IsCaption)
                    col.CanEmpty = false;
                if (col.LinkTable != null)
                {
                    col.IsUserReadOnly = col.IsLinkKey;
                    col.CanEmpty = true;
                }
                if (col.IsMemo || col.IsBlob)
                    col.CanEmpty = true;
                if (!repair)
                    continue;
                switch (col.Name)
                {
                    case "ParentId":
                    case "AuthorID":
                    case "AddDate":
                        col.Nullable = false;
                        //col.DbNullable = false;
                        col.IsUserReadOnly = true;
                        col.KeepStorageScreen = StorageScreenType.Update;
                        break;
                }
            }
        }
        #endregion
        #region NewEdit

        private void CheckType(PropertyConfig column, string type)
        {
            if (type[type.Length - 1] == '?')
            {
                column.Nullable = true;
                type = type.TrimEnd('?');
            }
            var tp = type.TrimEnd('?').ToLower();
            switch (tp)
            {
                case "t":
                case "s":
                case "string":
                case "nvarchar":
                    column.CsType = "string";
                    column.DbType = "NVARCHAR";
                    break;
                case "b":
                case "bool":
                    column.CsType = "bool";
                    column.DbType = "BOOL";
                    break;
                case "i":
                case "int":
                    column.CsType = "int";
                    column.DbType = "int";
                    break;
                case "l":
                case "long":
                case "bigint":
                    column.CsType = "long";
                    column.DbType = "BIGINT";
                    break;
                case "d":
                case "decimal":
                case "money":
                case "numeric":
                    column.CsType = "decimal";
                    column.DbType = "decimal";
                    break;
                case "f":
                case "float":
                    column.CsType = "double";
                    column.DbType = "double";
                    break;
                case "p":
                    column.IsPrimaryKey = true;
                    column.CsType = "int";
                    column.DbType = "int";
                    break;
                case "u":
                    column.IsUserId = true;
                    column.CsType = "int";
                    column.DbType = "int";
                    break;
                case "datetime":
                    column.IsUserId = true;
                    column.CsType = "DateTime";
                    column.DbType = "DateTime";
                    break;
                default:
                    column.CsType = tp;
                    column.DbType = tp;
                    break;
            }
        }

        /// <summary>
        /// 检查字段
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<PropertyConfig> DoCheckFieldes(string text)
        {
            var columns = new List<PropertyConfig>();
            string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            int idx = 0;

            foreach (string line in lines)
            {
                if (string.IsNullOrEmpty(line))
                    continue;
                string[] words = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                var name = words[0].ToUWord();
                if (idx == 0)
                {
                    idx = 1;
                    if (name[0] == '*')
                    {
                        Entity.ReadTableName = Entity.Name = name.Substring(1);

                        if (words.Length > 1)
                        {
                            Entity.ReadTableName = words[1];
                        }
                        if (words.Length <= 2)
                            continue;
                        Entity.Caption = words[2];
                        Entity.Description = words.Skip(2).LinkToString(",");
                        continue;
                    }
                }
                /*
                 文本说明:
                 * 1 每行为一条数据
                 * 2 每个单词用空格,逗号分开
                 * 3 第一个单词 代码名称; 第二个单词 数据类型;第三个单词 说明文本
                 */
                PropertyConfig column;
                columns.Add(column = new PropertyConfig
                {
                    Index = idx++,
                    IsPrimaryKey = name.Equals("ID", StringComparison.OrdinalIgnoreCase),
                    ColumnName = name,
                    Name = name,
                    CsType = "string",
                    DbType = "nvarchar"
                });
                if (words.Length > 1)
                {
                    CheckType(column, words[1]);
                }
                if (words.Length <= 2)
                    continue;
                column.Caption = words[2];
                column.Description = words.Skip(2).LinkToString(",");
            }
            return columns;
        }
        #endregion

        #region 命令

        /// <summary>
        ///     复制值
        /// </summary>
        /// <param name="source">复制的源字段</param>
        /// <param name="noChilds">是否复制子级(默认为是)</param>
        public void CopyValue(EntityConfig source, bool noChilds = false)
        {
            Entity.Caption = source.Caption + "(复制)";
            Entity.Description = source.Description;
            Entity.Name = source.Name + "_copy";
            Entity.DataVersion = source.DataVersion;
            Entity.DbIndex = source.DbIndex;
            Entity.IsInternal = source.IsInternal;
            Entity.IsClass = source.IsClass;
            Entity.ReadTableName = source.ReadTableName;
            Entity.CppName = source.CppName;
            Entity.Project = source.Project;
            if (noChilds)
            {
                return;
            }
            foreach (var field in source.Properties)
            {
                var nf = new PropertyConfig();
                nf.CopyFrom(field);
                Entity.Properties.Add(nf);
            }
        }

        #endregion

    }
}


