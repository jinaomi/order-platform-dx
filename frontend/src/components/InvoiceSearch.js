import { useState } from "react";
import LoadingSpinner from "./until/LoadingSpinner";
import FormButton from "./until/FormButton";
import useAxiosPrivate from "../hooks/useAxiosPrivate";
import Pagination from "./until/Pagination";
import commonState from "../stories/commonState.ts";
import commonActions from "../actions/commonAction.ts";
import {
  Button,
  Chip,
  Grid,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from "@mui/material";
import invoiceService from "../services/invoiceService";
import * as Icons from "@mui/icons-material";
import "../styles/styles.css";
import FormSnackbar from "./until/FormSnackbar.js";

const statusColor = {
  Draft: "default",
  Issued: "info",
  Paid: "success",
  Overdue: "error",
};

const InvoiceSearch = () => {
  const [showList, setShowList] = useState(false);
  const [listItem, setListItem] = useState({ items: [] });
  const [loading, setLoading] = useState(false);
  const axiosPrivate = useAxiosPrivate();
  const [snackbar, setSnackbar] = useState({
    isOpen: false,
    status: "success",
    message: "Successfully!",
  });

  const getInvoices = async (e) => {
    setLoading(true);
    if (e) e.preventDefault();
    await invoiceService
      .getAll(
        axiosPrivate,
        commonState.paginationState.pageSize,
        commonState.paginationState.currentPage
      )
      .then((response) => {
        setListItem(response.data);
        commonActions.setPaginationState({
          ...commonState.paginationState,
          totalCount: response.data.totalCount,
        });
      })
      .catch(() => {
        setListItem({ items: [] });
      });
    setLoading(false);
  };

  const handleClickSearch = async (e) => {
    await getInvoices(e);
    setShowList(true);
  };

  const handleDownload = async (invoice) => {
    setLoading(true);
    try {
      const response = await invoiceService.download(axiosPrivate, invoice.id);
      const url = window.URL.createObjectURL(new Blob([response.data], { type: "application/pdf" }));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute("download", `${invoice.invoiceNumber}.pdf`);
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      setSnackbar({
        isOpen: true,
        status: "error",
        message: "請求書のダウンロードに失敗しました。",
      });
    }
    setLoading(false);
  };

  const handleMarkPaid = async (invoice) => {
    setLoading(true);
    try {
      await invoiceService.updateStatus(axiosPrivate, invoice.id, "Paid");
      await getInvoices();
      setSnackbar({ isOpen: true, status: "success", message: "入金済みに更新しました。" });
    } catch (error) {
      setSnackbar({ isOpen: true, status: "error", message: "更新に失敗しました。" });
    }
    setLoading(false);
  };

  const handleChangePageSize = async (e) => {
    commonActions.setPaginationState({
      ...commonState.paginationState,
      pageSize: parseInt(e.target.value),
    });
    await getInvoices(e);
  };
  const handleChangePage = async (e, value) => {
    commonActions.setPaginationState({
      ...commonState.paginationState,
      currentPage: value,
    });
    await getInvoices(e);
  };

  const Results = () => {
    let totalCount = 0;
    if (commonState.paginationState && commonState.paginationState.totalCount > 0) {
      totalCount = Math.ceil(
        commonState.paginationState.totalCount / commonState.paginationState.pageSize
      );
    }
    return (
      <>
        <Pagination
          totalCount={totalCount}
          pageSize={commonState.paginationState.pageSize}
          currentPage={commonState.paginationState.currentPage}
          handleChangePageSize={handleChangePageSize}
          handleChangePage={handleChangePage}
        />
        <TableContainer component={Paper}>
          <Table sx={{ minWidth: 650 }} aria-label="simple table">
            <TableHead>
              <TableRow>
                <TableCell style={{ textAlign: "center" }}>請求書番号</TableCell>
                <TableCell style={{ textAlign: "center" }}>受注番号</TableCell>
                <TableCell style={{ textAlign: "center" }}>取引先</TableCell>
                <TableCell style={{ textAlign: "center" }}>発行日</TableCell>
                <TableCell style={{ textAlign: "center" }}>合計金額</TableCell>
                <TableCell style={{ textAlign: "center" }}>ステータス</TableCell>
                <TableCell style={{ textAlign: "center" }}>操作</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {listItem && listItem.items && listItem.items.length > 0 ? (
                listItem.items.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell>{item.invoiceNumber}</TableCell>
                    <TableCell>{item.orderNumber}</TableCell>
                    <TableCell>{item.customerName}</TableCell>
                    <TableCell>{item.issueDate ? item.issueDate.slice(0, 10) : ""}</TableCell>
                    <TableCell style={{ textAlign: "right" }}>
                      {item.totalAmount != null ? item.totalAmount.toLocaleString() : ""}
                    </TableCell>
                    <TableCell style={{ textAlign: "center" }}>
                      <Chip label={item.status} color={statusColor[item.status] || "default"} size="small" />
                    </TableCell>
                    <TableCell style={{ textAlign: "center" }}>
                      <Button
                        variant="contained"
                        color="success"
                        startIcon={<Icons.Download />}
                        onClick={() => handleDownload(item)}
                        style={{ margin: "1px 5px" }}
                      >
                        PDF
                      </Button>
                      {item.status !== "Paid" && (
                        <Button
                          variant="contained"
                          color="success"
                          startIcon={<Icons.Paid />}
                          style={{ margin: "1px 5px" }}
                          onClick={() => handleMarkPaid(item)}
                        >
                          入金済みにする
                        </Button>
                      )}
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={7}>
                    <span style={{ color: "#000" }}>表示する項目がありません。</span>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </>
    );
  };

  return (
    <section>
      <Grid container spacing={5}>
        <Grid item xs={12}>
          <div className="handle-button">
            <FormButton itemName="検索" onClick={handleClickSearch} />
          </div>
        </Grid>
        {showList ? (
          <Grid item xs={12}>
            <Results />
          </Grid>
        ) : null}
      </Grid>

      <LoadingSpinner loading={loading}></LoadingSpinner>
      <FormSnackbar item={snackbar} setItem={setSnackbar} />
    </section>
  );
};

export default InvoiceSearch;
