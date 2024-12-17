using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace UKSF.Launcher.Patching {
    internal class DownloadManager {
        private const string PASSWORD = "sneakysnek";
        private const string USERNAME = "launcher";
        private static DateTime lastReport = DateTime.Now;
        private long _bytesDone, _bytesTotal, _bytesRate;

        private ConcurrentQueue<DownloadTask> _downloadQueue;
        private Task _downloadTask;

        public DownloadManager() {
            _downloadTask = null;
            _downloadQueue = new ConcurrentQueue<DownloadTask>();
        }

        public static event EventHandler<string> LogEvent;
        public static event EventHandler<Tuple<float, string>> ProgressEvent;

        public void AddDownloadTask(string localPath, string remotePath, Action callbackAction, CancellationToken cancellationToken) {
            DownloadTask task = new DownloadTask(localPath, remotePath, callbackAction, cancellationToken);
            _downloadQueue.Enqueue(task);
            task.DownloadProgressEvent += (sender, change) => ProgressAction(change);
            long fileSize = task.GetRemoteFileSize();
            while (fileSize == -1) {
                fileSize = task.GetRemoteFileSize();
            }

            _bytesTotal += fileSize;
        }

        public void ProcessDownloadQueue(CancellationToken downloadCancellationToken) {
            if (_downloadTask != null) return;
            Task.Run(() => {
                         try {
                             _downloadTask = Task.Run(() => {
                                                          while (_downloadQueue.Count > 0 && !downloadCancellationToken.IsCancellationRequested) {
                                                              if (!_downloadQueue.TryDequeue(out DownloadTask task)) continue;
                                                              while (!downloadCancellationToken.IsCancellationRequested) {
                                                                  if (!task.Download()) continue;
                                                                  task.FinishDownload();
                                                                  break;
                                                              }
                                                          }
                                                      },
                                                      downloadCancellationToken);
                             _downloadTask.Wait(downloadCancellationToken);
                         } catch {
                             // ignored
                         } finally {
                             _downloadTask = null;
                         }
                     },
                     downloadCancellationToken);
        }

        public bool IsDownloadQueueEmpty() => _downloadQueue.Count == 0;

        public void ResetDownloadQueue() {
            _downloadQueue = new ConcurrentQueue<DownloadTask>();
            _bytesDone = 0;
            _bytesTotal = 0;
            _bytesRate = 0;
        }

        private static void LogAction(string message) {
            LogEvent?.Invoke(null, message);
        }

        private void ProgressAction(long change) {
            _bytesDone += change;
            _bytesRate += change;
            if (DateTime.Now < lastReport) return;
            lastReport = DateTime.Now.AddMilliseconds(100);
            ProgressEvent?.Invoke(this,
                                  new Tuple<float, string>((float) _bytesDone / _bytesTotal,
                                                           $"Downloading \n{Utility.ByteSize(_bytesDone)} / {Utility.ByteSize(_bytesTotal)} ({(int) (_bytesRate * 0.000078125)} Mbps)"));
            _bytesRate = 0L;
            // Mbps = rate * 80 / 1024 / 1000
        }

        public static void UploadFile(string localPath, string remotePath, CancellationToken downloadCancellationToken) {
            try {
                LogAction($"File '{Utility.ByteSize(new FileInfo(localPath).Length)}'");
                using (WebClient webClient = new WebClient()) {
                    downloadCancellationToken.Register(webClient.CancelAsync);
                    webClient.Credentials = new NetworkCredential(USERNAME, PASSWORD);
                    webClient.UploadFile(new Uri(remotePath), "STOR", localPath);
                }

                LogAction($"Uploaded '{localPath}'");
            } catch (Exception exception) {
                LogAction($"An error occured uploading '{localPath}'\n{exception}");
            } finally {
                File.Delete(localPath);
            }
        }

        private class DownloadTask {
            private readonly Action _callbackAction;
            private readonly CancellationToken _downloadCancellationToken;
            private readonly string _localPath;
            private readonly string _remotePath;

            public DownloadTask(string localPath, string remotePath, Action callbackAction, CancellationToken cancellationToken) {
                _localPath = localPath;
                _remotePath = remotePath;
                _callbackAction = callbackAction;
                _downloadCancellationToken = cancellationToken;
            }

            public event EventHandler<long> DownloadProgressEvent;

            public long GetRemoteFileSize() {
                try {
                    FtpWebRequest ftpWebRequest = (FtpWebRequest) WebRequest.Create($"ftp://arma.uk-sf.com/{_remotePath}");
                    ftpWebRequest.KeepAlive = true;
                    ftpWebRequest.UsePassive = true;
                    ftpWebRequest.Credentials = new NetworkCredential(USERNAME, PASSWORD);
                    ftpWebRequest.ConnectionGroupName = USERNAME;
                    ftpWebRequest.Method = WebRequestMethods.Ftp.GetFileSize;
                    ftpWebRequest.Timeout = Timeout.Infinite;
                    using (FtpWebResponse ftpWebResponse = (FtpWebResponse) ftpWebRequest.GetResponse()) {
                        return ftpWebResponse.ContentLength;
                    }
                } catch (Exception) {
                    return -1;
                }
            }

            public bool Download() {
                try {
                    FtpWebRequest ftpWebRequest = (FtpWebRequest) WebRequest.Create($"ftp://arma.uk-sf.com/{_remotePath}");
                    ftpWebRequest.KeepAlive = true;
                    ftpWebRequest.UsePassive = true;
                    ftpWebRequest.UseBinary = true;
                    ftpWebRequest.Credentials = new NetworkCredential(USERNAME, PASSWORD);
                    ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                    ftpWebRequest.ConnectionGroupName = USERNAME;
                    ftpWebRequest.Timeout = Timeout.Infinite;
                    if (!Directory.Exists(Path.GetDirectoryName(_localPath))) {
                        Directory.CreateDirectory(Path.GetDirectoryName(_localPath));
                    }

                    using (FtpWebResponse ftpWebResponse = (FtpWebResponse) ftpWebRequest.GetResponse()) {
                        using (FileStream localFileStream = File.OpenWrite(_localPath)) {
                            using (Stream responseStream = ftpWebResponse.GetResponseStream()) {
                                byte[] buffer = new byte[65536];
                                int count;
                                do {
                                    count = responseStream.Read(buffer, 0, buffer.Length);
                                    localFileStream.Write(buffer, 0, count);
                                    DownloadProgressEvent?.Invoke(this, count);
                                } while (!_downloadCancellationToken.IsCancellationRequested && count > 0);
                            }
                        }
                    }

                    LogAction($"Downloaded '{_localPath}'");
                    return true;
                } catch (WebException webException) {
                    if (webException.Status != WebExceptionStatus.Timeout) {
                        LogAction($"An error occured downloading '{_remotePath}'\n{webException}");
                    }
                } catch (Exception exception) {
                    LogAction($"An error occured downloading '{_remotePath}'\n{exception}");
                }

                return false;
            }

            public void FinishDownload() {
                _callbackAction.Invoke();
            }
        }
    }
}
