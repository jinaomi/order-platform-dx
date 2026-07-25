const keywordService = {
  getByTemplate: (axiosPrivate, templateId) =>
    axiosPrivate.get(`/api/keywords?templateId=${templateId}`),
  create: (axiosPrivate, data) =>
    axiosPrivate.post('/api/keywords', data),
  update: (axiosPrivate, id, data) =>
    axiosPrivate.put(`/api/keywords/${id}`, data),
  softDelete: (axiosPrivate, id) =>
    axiosPrivate.delete(`/api/keywords/${id}`),
  reorder: (axiosPrivate, items) =>
    axiosPrivate.patch('/api/keywords/reorder', items),
};
export default keywordService;
