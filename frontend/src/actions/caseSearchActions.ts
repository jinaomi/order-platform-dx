import { state as caseSearchState } from '../stories/caseSearchState.ts';
import { GetCaseList} from '../services/caseSearchService.ts'

const setPaginationState = (totalCount, pageSize, currentPage) => {
    caseSearchState.paginationState.totalCount = totalCount;
    caseSearchState.paginationState.pageSize = pageSize;
    caseSearchState.paginationState.currentPage = currentPage;
};

const setKeywordsSearchState = (keywordValues) => {
    caseSearchState.keywordsSearchState.currentPage = caseSearchState.paginationState.currentPage;
    caseSearchState.keywordsSearchState.pageSize = caseSearchState.paginationState.pageSize;
    caseSearchState.keywordsSearchState.keywordValues = keywordValues;
};

const setCaseDataSearchState = (keywordDataResponse) => {
    caseSearchState.caseDataSearchState.currentPage = caseSearchState.paginationState.currentPage;
    caseSearchState.caseDataSearchState.pageSize = caseSearchState.paginationState.pageSize;
    caseSearchState.caseDataSearchState.totalCount = caseSearchState.paginationState.totalCount;
    caseSearchState.caseDataSearchState.items = keywordDataResponse;
};

const getCaseList = async () => {
    const payload = {
      keywordValues: caseSearchState.keywordsSearchState.keywordValues,
      pageSize: caseSearchState.paginationState.pageSize,
      pageNumber: caseSearchState.paginationState.currentPage
    }
    let payloadFilterd = caseSearchState.keywordsSearchState.keywordValues.filter(n => n['value']);
    payload.keywordValues = payloadFilterd;
    let response = await GetCaseList(payload);
    if(response) {
        setPaginationState(response.totalCount, response.pageSize, response.currentPage)
        setCaseDataSearchState(response.items)
    }
    return response;
};

const caseSearchActions = {
    setPaginationState,
    setKeywordsSearchState,
    setCaseDataSearchState,
    getCaseList,
};

export default caseSearchActions;