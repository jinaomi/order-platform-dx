import { useState, useEffect } from "react";
import {
  Alert,
  Card,
  CardContent,
  Chip,
  Grid,
  List,
  ListItem,
  ListItemText,
  Skeleton,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import AutoAwesomeIcon from "@mui/icons-material/AutoAwesome";
import LoadingSpinner from "./until/LoadingSpinner";
import useAxiosPrivate from "../hooks/useAxiosPrivate";
import dashboardService from "../services/dashboardService";
import "../styles/styles.css";

const statusColor = {
  Draft: "default",
  Confirmed: "success",
  RiskFlagged: "warning",
  Invoiced: "info",
  Cancelled: "error",
};

const StatTile = ({ label, value, color }) => (
  <Card>
    <CardContent>
      <Typography variant="body2" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="h4" style={{ color: color || "#11596F", fontWeight: "bold" }}>
        {value}
      </Typography>
    </CardContent>
  </Card>
);

const AiCommentCard = () => {
  const [comment, setComment] = useState(null);
  const [loading, setLoading] = useState(true);
  const axiosPrivate = useAxiosPrivate();

  useEffect(async () => {
    setLoading(true);
    try {
      const response = await dashboardService.getAiComment(axiosPrivate);
      setComment(response.data || null);
    } catch (error) {
      setComment(null);
    }
    setLoading(false);
  }, []);

  if (loading) {
    return (
      <Card>
        <CardContent>
          <Skeleton variant="text" width="40%" />
          <Skeleton variant="text" width="90%" />
          <Skeleton variant="text" width="80%" />
          <Skeleton variant="text" width="60%" />
        </CardContent>
      </Card>
    );
  }

  if (!comment) {
    return null;
  }

  return (
    <Card>
      <CardContent>
        <Typography
          variant="h6"
          gutterBottom
          style={{ display: "flex", alignItems: "center", gap: 8 }}
        >
          <AutoAwesomeIcon color="primary" /> AI経営コメント
        </Typography>
        <Typography variant="subtitle1" style={{ fontWeight: "bold" }} gutterBottom>
          {comment.headline}
        </Typography>
        <List dense>
          {comment.highlights.map((h, i) => (
            <ListItem key={i} style={{ display: "list-item", listStyleType: "disc", marginLeft: 20, padding: 0 }}>
              <ListItemText primary={h} />
            </ListItem>
          ))}
        </List>
        {comment.recommendation && (
          <Alert severity="info" style={{ marginTop: 10 }}>
            {comment.recommendation}
          </Alert>
        )}
      </CardContent>
    </Card>
  );
};

const SalesDashboard = () => {
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(false);
  const axiosPrivate = useAxiosPrivate();

  useEffect(async () => {
    setLoading(true);
    try {
      const response = await dashboardService.getSummary(axiosPrivate);
      setSummary(response.data);
    } catch (error) {
      setSummary(null);
    }
    setLoading(false);
  }, []);

  if (!summary) {
    return <LoadingSpinner loading={loading}></LoadingSpinner>;
  }

  return (
    <section>
      <Grid container spacing={3}>
        <Grid item xs={12}>
          <AiCommentCard />
        </Grid>

        <Grid item xs={12} sm={3}>
          <StatTile label="受注件数" value={summary.totalOrders} />
        </Grid>
        <Grid item xs={12} sm={3}>
          <StatTile
            label="受注金額合計"
            value={`¥${summary.totalOrderAmount.toLocaleString()}`}
          />
        </Grid>
        <Grid item xs={12} sm={3}>
          <StatTile
            label="請求済み金額"
            value={`¥${summary.totalInvoicedAmount.toLocaleString()}`}
            color="#0B78D1"
          />
        </Grid>
        <Grid item xs={12} sm={3}>
          <StatTile
            label="リスクあり受注件数"
            value={summary.riskFlaggedCount}
            color={summary.riskFlaggedCount > 0 ? "#c62828" : "#11596F"}
          />
        </Grid>

        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                受注ステータス内訳
              </Typography>
              {summary.orderFunnel.map((s) => (
                <Chip
                  key={s.status}
                  label={`${s.status}: ${s.count}件`}
                  color={statusColor[s.status] || "default"}
                  style={{ marginRight: 10, marginBottom: 5 }}
                />
              ))}
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                月別売上
              </Typography>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>年月</TableCell>
                    <TableCell style={{ textAlign: "right" }}>受注件数</TableCell>
                    <TableCell style={{ textAlign: "right" }}>金額</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {summary.monthlySales.map((m) => (
                    <TableRow key={m.month}>
                      <TableCell>{m.month}</TableCell>
                      <TableCell style={{ textAlign: "right" }}>{m.orderCount}</TableCell>
                      <TableCell style={{ textAlign: "right" }}>
                        ¥{m.totalAmount.toLocaleString()}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                取引先別売上 TOP5
              </Typography>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>取引先</TableCell>
                    <TableCell style={{ textAlign: "right" }}>受注件数</TableCell>
                    <TableCell style={{ textAlign: "right" }}>金額</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {summary.topCustomers.map((c) => (
                    <TableRow key={c.customerName}>
                      <TableCell>{c.customerName}</TableCell>
                      <TableCell style={{ textAlign: "right" }}>{c.orderCount}</TableCell>
                      <TableCell style={{ textAlign: "right" }}>
                        ¥{c.totalAmount.toLocaleString()}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                商品別売上 TOP5
              </Typography>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>商品名</TableCell>
                    <TableCell style={{ textAlign: "right" }}>数量</TableCell>
                    <TableCell style={{ textAlign: "right" }}>金額</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {summary.topProducts.map((p) => (
                    <TableRow key={p.productName}>
                      <TableCell>{p.productName}</TableCell>
                      <TableCell style={{ textAlign: "right" }}>{p.totalQuantity}</TableCell>
                      <TableCell style={{ textAlign: "right" }}>
                        ¥{p.totalAmount.toLocaleString()}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
      <LoadingSpinner loading={loading}></LoadingSpinner>
    </section>
  );
};

export default SalesDashboard;
