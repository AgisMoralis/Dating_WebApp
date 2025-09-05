export interface Pagination {
  currentPage: number;
  totalPages: number;
  itemsPerPage: number;
  totalItems: number;
}

export interface PaginatedResult<T> {
  items?: T;
  pagination?: Pagination;
}