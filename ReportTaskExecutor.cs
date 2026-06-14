using Newtonsoft.Json.Linq;
using System;
using System.Windows.Forms;

namespace GridReportForm
{
    internal class ReportTaskExecutor
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly Control uiControl;
        private readonly Func<bool> allowPreview;

        public ReportTaskExecutor(Control uiControl, Func<bool> allowPreview)
        {
            this.uiControl = uiControl;
            this.allowPreview = allowPreview;
        }

        public CloudTaskResult Execute(CloudReportTask task)
        {
            if (task == null || string.IsNullOrWhiteSpace(task.Cmd))
            {
                logger.Warn("Cloud task rejected because command is empty.");
                return CloudTaskResult.Fail(null, null, "指令未传输", "INVALID_TASK");
            }
            if (string.IsNullOrWhiteSpace(task.TemplateUrl))
            {
                logger.Warn("Cloud task rejected because template url is empty. TaskId={TaskId}, Cmd={Cmd}", task.TaskId, task.Cmd);
                return CloudTaskResult.Fail(task.TaskId, task.Cmd, "模板信息未传输", "TEMPLATE_REQUIRED");
            }

            try
            {
                logger.Info("Executing cloud task. TaskId={TaskId}, Cmd={Cmd}, TemplateUrl={TemplateUrl}, DataUrl={DataUrl}, PrinterName={PrinterName}", task.TaskId, task.Cmd, task.TemplateUrl, task.DataUrl, task.PrinterName);
                switch (task.Cmd)
                {
                    case "print":
                        ShowPrint(task);
                        logger.Info("Cloud print task submitted. TaskId={TaskId}, PrinterName={PrinterName}", task.TaskId, ResolvePrinterName(task));
                        return CloudTaskResult.Ok(task.TaskId, task.Cmd, "打印任务已提交", new { printerName = ResolvePrinterName(task) });
                    case "preview":
                        if (!allowPreview())
                        {
                            logger.Warn("Cloud preview task rejected because preview is disabled. TaskId={TaskId}", task.TaskId);
                            return CloudTaskResult.Fail(task.TaskId, task.Cmd, "当前设备未启用预览", "PREVIEW_NOT_ALLOWED");
                        }
                        ShowPreview(task);
                        logger.Info("Cloud preview task submitted. TaskId={TaskId}", task.TaskId);
                        return CloudTaskResult.Ok(task.TaskId, task.Cmd, "预览窗口已打开");
                    default:
                        logger.Warn("Unknown cloud task command. TaskId={TaskId}, Cmd={Cmd}", task.TaskId, task.Cmd);
                        return CloudTaskResult.Fail(task.TaskId, task.Cmd, $"未知云任务动作：{task.Cmd}", "UNKNOWN_COMMAND");
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Cloud task execution failed. TaskId={TaskId}, Cmd={Cmd}", task.TaskId, task.Cmd);
                return CloudTaskResult.Fail(task.TaskId, task.Cmd, exception.Message, "EXECUTION_FAILED");
            }
        }

        private void ShowPrint(CloudReportTask task)
        {
            uiControl.Invoke((MethodInvoker)delegate
            {
                logger.Debug("Opening print form. TaskId={TaskId}, TemplateUrl={TemplateUrl}, DataUrl={DataUrl}", task.TaskId, task.TemplateUrl, task.DataUrl);
                Form form = new PrintForm(task.TemplateUrl, task.DataUrl, BuildExtInfo(task));
                form.Show();
                form.TopMost = true;
                form.Activate();
                form.TopMost = false;
            });
        }

        private void ShowPreview(CloudReportTask task)
        {
            uiControl.Invoke((MethodInvoker)delegate
            {
                logger.Debug("Opening preview form. TaskId={TaskId}, TemplateUrl={TemplateUrl}, DataUrl={DataUrl}", task.TaskId, task.TemplateUrl, task.DataUrl);
                Form form = new PreviewForm(task.TemplateUrl, task.DataUrl, BuildExtInfo(task));
                form.Show();
                form.TopMost = true;
                form.Activate();
                form.TopMost = false;
            });
        }

        private static JObject BuildExtInfo(CloudReportTask task)
        {
            JObject extInfo = task.ExtInfo == null ? new JObject() : new JObject(task.ExtInfo);
            extInfo["printerName"] = ResolvePrinterName(task);
            if (extInfo["showPrintDialog"] == null)
            {
                extInfo["showPrintDialog"] = false;
            }
            if (extInfo["id"] == null && !string.IsNullOrWhiteSpace(task.TaskId))
            {
                extInfo["id"] = task.TaskId;
            }
            return extInfo;
        }

        private static string ResolvePrinterName(CloudReportTask task)
        {
            return string.IsNullOrWhiteSpace(task.PrinterName) ? MyLocalPrinter.DefaultPrinter() : task.PrinterName;
        }
    }
}
