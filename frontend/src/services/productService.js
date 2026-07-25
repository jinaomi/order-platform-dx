const getAll = (axiosPrivate, name, pageSize = 25, pageNumber = 1) => {
  let url = `/api/product/getAll?pageSize=${pageSize}&pageNumber=${pageNumber}`;
  if (name) {
    url += `&name=${name}`;
  }
  return axiosPrivate.get(url);
};

const list = (axiosPrivate) => {
  return axiosPrivate.get("/api/product/list");
};

const getById = (axiosPrivate, id) => {
  return axiosPrivate.get(`/api/product?id=${id}`);
};

const create = (axiosPrivate, data) => {
  return axiosPrivate.post("/api/product", data);
};

const update = (axiosPrivate, id, data) => {
  return axiosPrivate.put(`/api/product/${id}`, data);
};

const deleteById = (axiosPrivate, id) => {
  return axiosPrivate.delete(`/api/product/${id}`);
};

const productService = { getAll, list, getById, create, update, deleteById };

export default productService;
