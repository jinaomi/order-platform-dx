import { createStore } from "@stencil/store";
import iPagination from "../interfaces/iPagination.ts";

const paginationState: iPagination = {
  totalCount: 0,
  currentPage: 1,
  pageSize: 10,
};

const keywordsSearchState = {
  currentPage: paginationState.currentPage,
  pageSize: paginationState.pageSize,
  keywordValues: [],
  keywordDateValues: [],
};

const caseDataSearchState = {
  totalCount: paginationState.totalCount,
  currentPage: paginationState.currentPage,
  pageSize: paginationState.pageSize,
  items: [],
};

const caseSearchState = {
  paginationState,
  keywordsSearchState,
  caseDataSearchState,
};

const { state, on, onChange, reset } = createStore(caseSearchState);
export { caseSearchState, state, on, onChange, reset };
export default state;
