const getAll = (
  axiosPrivate,
  status,
  customerId,
  orderDateFrom,
  orderDateTo,
  pageSize = 25,
  pageNumber = 1
) => {
  let url = `/api/order/getAll?pageSize=${pageSize}&pageNumber=${pageNumber}`;
  if (status) {
    url += `&status=${status}`;
  }
  if (customerId) {
    url += `&customerId=${customerId}`;
  }
  if (orderDateFrom) {
    url += `&orderDateFrom=${orderDateFrom}`;
  }
  if (orderDateTo) {
    url += `&orderDateTo=${orderDateTo}`;
  }
  return axiosPrivate.get(url);
};

const getById = (axiosPrivate, id) => {
  return axiosPrivate.get(`/api/order?id=${id}`);
};

const create = (axiosPrivate, data) => {
  return axiosPrivate.post("/api/order", data);
};

const update = (axiosPrivate, id, data) => {
  return axiosPrivate.put(`/api/order/${id}`, data);
};

const updateStatus = (axiosPrivate, id, status) => {
  return axiosPrivate.put(`/api/order/status?id=${id}&status=${status}`);
};

const deleteById = (axiosPrivate, id) => {
  return axiosPrivate.delete(`/api/order/${id}`);
};

const extract = (axiosPrivate, file) => {
  const formData = new FormData();
  formData.append("file", file);
  return axiosPrivate.post("/api/order/extract", formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
};

const orderService = { getAll, getById, create, update, updateStatus, deleteById, extract };

export default orderService;
