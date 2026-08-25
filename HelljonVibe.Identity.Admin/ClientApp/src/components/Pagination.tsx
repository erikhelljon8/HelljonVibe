import React from 'react';
import { Pagination, PaginationItem, PaginationLink } from 'reactstrap';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

const PaginationComponent: React.FC<PaginationProps> = ({ currentPage, totalPages, onPageChange }) => {
  if (totalPages <= 1) {
    return null;
  }

  const getPageNumbers = () => {
    const pages: (number | string)[] = [];
    const showEllipsis = totalPages > 7;

    if (!showEllipsis) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      // Always show first page
      pages.push(1);

      if (currentPage > 3) {
        pages.push('...');
      }

      // Show pages around current page
      const start = Math.max(2, currentPage - 1);
      const end = Math.min(totalPages - 1, currentPage + 1);

      for (let i = start; i <= end; i++) {
        pages.push(i);
      }

      if (currentPage < totalPages - 2) {
        pages.push('...');
      }

      // Always show last page
      pages.push(totalPages);
    }

    return pages;
  };

  return (
    <Pagination className="justify-content-center mt-4">
      <PaginationItem disabled={currentPage === 1}>
        <PaginationLink first onClick={() => onPageChange(1)} />
      </PaginationItem>
      <PaginationItem disabled={currentPage === 1}>
        <PaginationLink previous onClick={() => onPageChange(currentPage - 1)} />
      </PaginationItem>

      {getPageNumbers().map((page, index) => (
        <PaginationItem key={index} active={page === currentPage}>
          {typeof page === 'number' ? (
            <PaginationLink onClick={() => onPageChange(page)}>
              {page}
            </PaginationLink>
          ) : (
            <PaginationLink disabled>
              {page}
            </PaginationLink>
          )}
        </PaginationItem>
      ))}

      <PaginationItem disabled={currentPage === totalPages}>
        <PaginationLink next onClick={() => onPageChange(currentPage + 1)} />
      </PaginationItem>
      <PaginationItem disabled={currentPage === totalPages}>
        <PaginationLink last onClick={() => onPageChange(totalPages)} />
      </PaginationItem>
    </Pagination>
  );
};

export default PaginationComponent;
