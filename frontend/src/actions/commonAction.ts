import { state as commonState } from '../stories/commonState.ts';
import iPagination from '../interfaces/iPagination.ts';

const setPaginationState = (pagination: iPagination) => {
    commonState.paginationState.totalCount = pagination.totalCount;
    commonState.paginationState.pageSize = pagination.pageSize;
    commonState.paginationState.currentPage = pagination.currentPage;
};

const commonActions = {
    setPaginationState,
};

export default commonActions;