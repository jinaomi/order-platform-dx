const getAll = (
  axiosPrivate,
  customerId,
  status,
  orderNumber,
  issueDateFrom,
  issueDateTo,
  pageSize = 25,
  pageNumber = 1
) => {
  let url = `/api/invoice/getAll?pageSize=${pageSize}&pageNumber=${pageNumber}`;
  if (customerId) {
    url += `&customerId=${customerId}`;
  }
  if (status) {
    url += `&status=${status}`;
  }
  if (orderNumber) {
    url += `&orderNumber=${encodeURIComponent(orderNumber)}`;
  }
  if (issueDateFrom) {
    url += `&issueDateFrom=${issueDateFrom}`;
  }
  if (issueDateTo) {
    url += `&issueDateTo=${issueDateTo}`;
  }
  return axiosPrivate.get(url);
};

const getById = (axiosPrivate, id) => {
  return axiosPrivate.get(`/api/invoice?id=${id}`);
};

const createFromOrder = (axiosPrivate, orderId) => {
  return axiosPrivate.post(`/api/invoice/from-order/${orderId}`);
};

const getByOrderId = (axiosPrivate, orderId) => {
  return axiosPrivate.get(`/api/invoice/by-order/${orderId}`);
};

const updateStatus = (axiosPrivate, id, status) => {
  return axiosPrivate.put(`/api/invoice/status?id=${id}&status=${status}`);
};

const download = (axiosPrivate, id) => {
  return axiosPrivate.get(`/api/invoice/${id}/download`, { responseType: "blob" });
};

const invoiceService = { getAll, getById, createFromOrder, getByOrderId, updateStatus, download };

export default invoiceService;
