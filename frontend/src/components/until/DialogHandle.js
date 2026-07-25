import Upload from "./Upload";
import LoadingSpinner from "./LoadingSpinner";
import {
  Button,
  Checkbox,
  Dialog,
  DialogContent,
  DialogContentText,
  DialogTitle,
  Grid,
  IconButton,
} from "@mui/material";
import { useState, useEffect } from "react";
import Truncate from "./Truncate";
import useAxiosPrivate from "../../hooks/useAxiosPrivate";
import CircularProgress from "@mui/material/CircularProgress";
import ConfirmDialog from "./ConfirmBox";
import * as Icons from "@mui/icons-material";
import ContentDialog from "../until/ContentDialog.js";
import FormSnackbar from "./FormSnackbar.js";

const DialogHandle = ({ title, open, closeDialog, optionFileType, caseId }) => {
  const axiosPrivate = useAxiosPrivate();
  const controller = new AbortController();
  const [loading, setLoading] = useState(false);
  const [loadingFile, setLoadingFile] = useState(false);
  const [listItem, setListItem] = useState([]);
  const [urlPreviewImg, setUrlPreviewImg] = useState({
    blobUrl: "",
    fileName: "",
  });
  const [fileDelete, setFileDelete] = useState({});
  const [showAlert, setShowAlert] = useState(false);
  const [showDialog, setShowDialog] = useState(false);
  const [selectedFiles, setSelectedFiles] = useState([]);
  const [showBulkDeleteAlert, setShowBulkDeleteAlert] = useState(false);

  const [dataUpload, setDataUpload] = useState({
    fileTypeId: null,
    fileName: "",
  });
  const [snackbar, setSnackbar] = useState({
    isOpen: false,
    status: "success",
    message: "Successfully!",
  });

  useEffect(async () => {
    setShowDialog(false);
    setListItem([]);
    setLoading(false);
    setLoadingFile(false);
    setUrlPreviewImg({ blobUrl: "", fileName: "" });
    setFileDelete({});
    setShowAlert(false);
    setSelectedFiles([]);
    setShowBulkDeleteAlert(false);
    await getFilesOfCase();
  }, [open]);

  const getFilesOfCase = async () => {
    setLoadingFile(true);
    setSelectedFiles([]);
    let getFilesUploadURL = `/api/Case/file/getall?caseId=${caseId}`;
    await axiosPrivate
      .get(getFilesUploadURL, {
        signal: controller.signal,
        validateStatus: () => true,
      })
      .then((response) => {
        if (response.data.status === 404) {
          setListItem([]);
        } else {
          setListItem(response.data);
        }
        return response;
      })
      .catch((error) => {
        setSnackbar({
          isOpen: true,
          status: "error",
          message:
            "エラーが発生しました。再試行するか、サポートにお問い合わせください。",
        });
      });
    setLoadingFile(false);
  };

  const handleToggleSelect = (keywordId) => {
    setSelectedFiles((prev) =>
      prev.includes(keywordId) ? prev.filter((id) => id !== keywordId) : [...prev, keywordId]
    );
  };

  const handleToggleSelectAll = () => {
    if (!listItem || listItem.length === 0) return;
    const allIds = listItem.map((f) => f.keywordId);
    const allSelected = allIds.every((id) => selectedFiles.includes(id));
    setSelectedFiles(allSelected ? [] : allIds);
  };

  const handleBulkDelete = async (e) => {
    e.preventDefault();
    setLoading(true);
    const payload = listItem
      .filter((f) => selectedFiles.includes(f.keywordId))
      .map((f) => ({ keywordId: f.keywordId, caseId: caseId, fileName: f.fileName }));
    await axiosPrivate
      .put("/api/FileUpload/BulkDelete", payload)
      .then(async () => {
        await getFilesOfCase();
        setShowBulkDeleteAlert(false);
        setSnackbar({ isOpen: true, status: "success", message: "選択した書類を削除しました。" });
      })
      .catch(() => {
        setSnackbar({
          isOpen: true,
          status: "error",
          message: "エラーが発生しました。再試行するか、サポートにお問い合わせください。",
        });
      });
    setLoading(false);
  };

  const uploadFunction = async (event) => {
    setLoading(true);
    setUrlPreviewImg({});
    event.preventDefault();

    const formData = new FormData();
    formData.append("FileToUpload", dataUpload.fileToUpload);
    formData.append("CaseId", caseId);
    formData.append("FileTypeId", dataUpload.fileTypeId);
    formData.append("FileName", dataUpload.fileName);

    await axiosPrivate
      .post("/api/FileUpload/Upload", formData)
      .then(async (response) => {
        await getFilesOfCase();
        setSnackbar({
          isOpen: true,
          status: "success",
          message: "書類は正常に添付されました。",
        });
      })
      .catch((error) => {
        if (error.response.data === "Your file is not supported")
          setSnackbar({
            isOpen: true,
            status: "error",
            message: "選択されたファイルの拡張子は利用できません。",
          });
        else
          setSnackbar({
            isOpen: true,
            status: "error",
            message:
              "エラーが発生しました。再試行するか、サポートにお問い合わせください。",
          });
      });
    setLoading(false);
    controller.abort();
  };

  const handleSelectedFileType = (e, value) => {
    var newState = {
      ...dataUpload,
      fileTypeId: value ? (value.id ? value.id : null) : null,
    };
    setDataUpload(newState);
  };
  const handleInputFileName = (e) => {
    var newState = { ...dataUpload, fileName: e.target.value };
    setDataUpload(newState);
  };
  const handleFileChange = (e) => {
    var newState = { ...dataUpload, fileToUpload: e.target.files[0] };
    setDataUpload(newState);
  };
  const viewOrDownloadFile = async (item, type) => {
    setLoading(true);
    let getFileUrl = `/api/FileUpload/Download`;
    let payload = {
      fileName: item.fileName,
      caseId: caseId,
    };
    await axiosPrivate
      .post(getFileUrl, payload)
      .then(async (response) => {
        const byteArray = Uint8Array.from(
          atob(response.data)
            .split("")
            .map((char) => char.charCodeAt(0))
        );
        const blob = new Blob([byteArray], {
          type: response.headers["content-type"],
        });
        const blobUrl = window.URL.createObjectURL(blob);
        if (type === "download") {
          const link = document.createElement("a");
          link.href = blobUrl;
          link.download = item.fileName;
          link.click();
        } else {
          setShowDialog(true);
          setUrlPreviewImg({ blobUrl: blobUrl, fileName: item.fileName });
        }
      })
      .catch(() => {
        setSnackbar({
          isOpen: true,
          status: "error",
          message:
            "エラーが発生しました。再試行するか、サポートにお問い合わせください。",
        });
      });
    setLoading(false);
  };
  const handleClickDelete = async (e) => {
    setLoading(true);
    e.preventDefault();
    let deleteFileUrl = `/api/FileUpload/Delete`;
    let payload = fileDelete;
    payload.caseId = caseId;
    await axiosPrivate
      .put(deleteFileUrl, payload)
      .then(async (response) => {
        setUrlPreviewImg({ ...urlPreviewImg, blobUrl: "", fileName: "" });
        await getFilesOfCase();
        setShowAlert(false);
        setSnackbar({
          isOpen: true,
          status: "success",
          message: "書類は正常に削除されました。",
        });
      })
      .catch(() => {
        setSnackbar({
          isOpen: true,
          status: "error",
          message:
            "エラーが発生しました。再試行するか、サポートにお問い合わせください。",
        });
      });
    setLoading(false);
  };
  return (
    <Dialog
      fullWidth={true}
      open={open}
      onClose={closeDialog}
      onBackdropClick={closeDialog}
      maxWidth="xl"
    >
      <DialogTitle>{title}</DialogTitle>
      <IconButton
        size="small"
        onClick={closeDialog}
        sx={{
          position: "absolute",
          right: "5px",
          top: "5px",
          width: "2rem",
          height: "2rem",
        }}
      >
        <Icons.Close sx={{ color: "red" }} />
      </IconButton>
      <DialogContent
        sx={{ px: 4, py: 6, position: "relative", minHeight: 960 }}
        style={{ paddingTop: "5px" }}
      >
        <DialogContentText style={{ color: "red", marginBottom: "20px" }}>
          利用可能なファイル拡張子：.jpeg,.jpg,.png,.gif,.tiff,.tif,.psd,.pdf,.eps,.raw,.txt,.doc,.docx,.xls,.xlsx,.ppt,.pptx,.dwg,.dxf,.jww
        </DialogContentText>
        <Grid container spacing={5}>
          <Grid item xs={4}>
            <Upload
              optionFileType={optionFileType}
              caseId={caseId}
              valueTypeId={dataUpload.fileTypeId}
              valueFileName={dataUpload.fileName}
              uploadFunction={uploadFunction}
              handleSelectedFileType={handleSelectedFileType}
              handleInputFileName={handleInputFileName}
              handleFileChange={handleFileChange}
            />
          </Grid>
          <Grid item xs={8}>
            {listItem && listItem[0] != null && (
              <div style={{ display: "flex", alignItems: "center", marginBottom: 4, gap: 8 }}>
                <Checkbox
                  checked={listItem.length > 0 && listItem.every((f) => selectedFiles.includes(f.keywordId))}
                  indeterminate={selectedFiles.length > 0 && !listItem.every((f) => selectedFiles.includes(f.keywordId))}
                  onChange={handleToggleSelectAll}
                />
                <span>全選択</span>
                {selectedFiles.length > 0 && (
                  <Button
                    variant="contained"
                    color="error"
                    startIcon={<Icons.Delete />}
                    onClick={() => setShowBulkDeleteAlert(true)}
                  >
                    選択した書類を削除 ({selectedFiles.length}件)
                  </Button>
                )}
              </div>
            )}
            <ul
              id="results"
              className="search-results"
              style={{ marginTop: 10 }}
            >
              {listItem && listItem[0] != null ? (
                listItem.map((item, index) => {
                  return (
                    <li className="search-result" key={item.keywordId}>
                      <Checkbox
                        checked={selectedFiles.includes(item.keywordId)}
                        onChange={() => handleToggleSelect(item.keywordId)}
                        style={{ padding: "0 4px 0 0" }}
                      />
                      <Truncate
                        str={item.fileName}
                        maxLength={20}
                        style={{ padding: "10px" }}
                      />
                      <div
                        className="search-action"
                        style={{
                          minWidth: 350,
                          display: "flex",
                          justifyContent: "flex-end",
                        }}
                      >
                        {item.isImage && (
                          <Button
                            className="search-delete"
                            onClick={async () => {
                              await viewOrDownloadFile(item, "view");
                            }}
                            startIcon={<Icons.Image />}
                            disabled={!item.isImage}
                          >
                            表示
                          </Button>
                        )}
                        <Button
                          startIcon={<Icons.Download />}
                          className="search-edit"
                          onClick={async () => {
                            await viewOrDownloadFile(item, "download");
                          }}
                        >
                          ダウンロード
                        </Button>
                        <Button
                          startIcon={<Icons.Delete />}
                          className="search-edit"
                          onClick={() => {
                            setFileDelete(item);
                            setShowAlert(true);
                          }}
                        >
                          削除
                        </Button>
                      </div>
                    </li>
                  );
                })
              ) : (
                <li style={{ textAlign: "center" }}>
                  {loadingFile ? (
                    <CircularProgress />
                  ) : (
                    <p>表示する項目がありません。</p>
                  )}
                </li>
              )}
            </ul>
          </Grid>
          {urlPreviewImg.blobUrl && (
            <ContentDialog
              open={showDialog}
              closeDialog={() => setShowDialog(false)}
            >
              <Grid item xs={12} className="preview-file">
                <a
                  href={urlPreviewImg.blobUrl}
                  download={urlPreviewImg.fileName}
                >
                  <IconButton size="small" aria-label="download">
                    <Icons.CloudDownload
                      sx={{ color: "green", fontSize: 40 }}
                    />
                  </IconButton>
                  書類のダウンロード
                </a>
                <img
                  src={urlPreviewImg.blobUrl}
                  style={{
                    width: "100%",
                    marginTop: "10px",
                    border: "3px solid #11596F",
                  }}
                />
              </Grid>
            </ContentDialog>
          )}
        </Grid>
        <ConfirmDialog
          open={showAlert}
          closeDialog={() => setShowAlert(false)}
          item={fileDelete.fileName}
          handleFunction={handleClickDelete}
          typeDialog="書類削除の確認"
          mainContent="書類を削除すると、案件から関連書類として参照できなくなります。本当に削除しますか"
          cancelBtnDialog="いいえ"
          confirmBtnDialog="はい"
        ></ConfirmDialog>
        <ConfirmDialog
          open={showBulkDeleteAlert}
          closeDialog={() => setShowBulkDeleteAlert(false)}
          item={`${selectedFiles.length}件の書類`}
          handleFunction={handleBulkDelete}
          typeDialog="書類一括削除の確認"
          mainContent="選択した書類を削除すると、案件から関連書類として参照できなくなります。本当に削除しますか"
          cancelBtnDialog="いいえ"
          confirmBtnDialog="はい"
        ></ConfirmDialog>
        <LoadingSpinner loading={loading}></LoadingSpinner>
      </DialogContent>
      <FormSnackbar item={snackbar} setItem={setSnackbar} />
    </Dialog>
  );
};

export default DialogHandle;
