import React from 'react';
import './Pagination.scss';

const Pagination = ({ 
  pageNumber, 
  totalPages, 
  totalCount,
  pageSize,
  onPageChange,
  onPageSizeChange 
}) => {
  const getPageNumbers = () => {
    const pages = [];
    const maxVisible = 5;
    
    let startPage = Math.max(1, pageNumber - Math.floor(maxVisible / 2));
    let endPage = Math.min(totalPages, startPage + maxVisible - 1);
    
    if (endPage - startPage < maxVisible - 1) {
      startPage = Math.max(1, endPage - maxVisible + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    
    return pages;
  };

  const startIndex = (pageNumber - 1) * pageSize + 1;
  const endIndex = Math.min(pageNumber * pageSize, totalCount);

  return (
    <div className="pagination-container">
      <div className="pagination-info">
        Showing <strong>{startIndex}</strong> to <strong>{endIndex}</strong> of <strong>{totalCount}</strong> entries
      </div>
      
      <div className="pagination-controls">
        <button 
          onClick={() => onPageChange(1)}
          disabled={pageNumber === 1}
          className="pagination-btn"
          title="First Page"
        >
          ««
        </button>

        <button 
          onClick={() => onPageChange(pageNumber - 1)}
          disabled={pageNumber === 1}
          className="pagination-btn"
        >
          ‹ Prev
        </button>

        {getPageNumbers().map(page => (
          <button
            key={page}
            onClick={() => onPageChange(page)}
            className={`pagination-btn ${page === pageNumber ? 'active' : ''}`}
          >
            {page}
          </button>
        ))}

        <button
          onClick={() => onPageChange(pageNumber + 1)}
          disabled={pageNumber === totalPages}
          className="pagination-btn"
        >
          Next ›
        </button>

        <button
          onClick={() => onPageChange(totalPages)}
          disabled={pageNumber === totalPages}
          className="pagination-btn"
          title="Last Page"
        >
          »»
        </button>
      </div>

      <div className="pagination-size">
        <label>Items per page:</label>
        <select 
          value={pageSize} 
          onChange={(e) => onPageSizeChange(Number(e.target.value))}
          className="pagination-select"
        >
          <option value={5}>5</option>
          <option value={10}>10</option>
          <option value={25}>25</option>
          <option value={50}>50</option>
          <option value={100}>100</option>
        </select>
      </div>
    </div>
  );
};

export default Pagination;