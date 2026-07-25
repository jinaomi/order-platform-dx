import { createStore } from "@stencil/store";
import iPagination from "../interfaces/iPagination.ts";

const paginationState: iPagination = {
  totalCount: 0,
  currentPage: 1,
  pageSize: 10,
};

const commonState = {
  paginationState,
};

const { state, on, onChange, reset } = createStore(commonState);
export { commonState, state, on, onChange, reset };
export default state;
