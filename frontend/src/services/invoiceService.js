const getAll = (axiosPrivate, pageSize = 25, pageNumber = 1) => {
  return axiosPrivate.get(`/api/invoice/getAll?pageSize=${pageSize}&pageNumber=${pageNumber}`);
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
