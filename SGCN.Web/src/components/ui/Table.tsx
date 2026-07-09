import type { ReactNode } from 'react';

type Column<T> = {
  header: string;
  cell: (item: T) => ReactNode;
  className?: string;
};

type TableProps<T> = {
  columns: Column<T>[];
  data: T[];
  emptyText?: string;
  keyExtractor: (item: T) => string;
};

export default function Table<T>({ columns, data, emptyText = 'Aucun élément trouvé.', keyExtractor }: TableProps<T>) {
  return (
    <div className="overflow-x-auto">
      <table className="min-w-full divide-y divide-slate-200 text-sm">
        <thead className="bg-slate-50 text-left text-xs font-semibold uppercase text-slate-500">
          <tr>
            {columns.map((column) => (
              <th className={`px-4 py-3 ${column.className ?? ''}`} key={column.header}>
                {column.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100 bg-white">
          {data.length === 0 ? (
            <tr>
              <td className="px-4 py-8 text-center text-slate-500" colSpan={columns.length}>
                {emptyText}
              </td>
            </tr>
          ) : (
            data.map((item) => (
              <tr className="align-top text-slate-700" key={keyExtractor(item)}>
                {columns.map((column) => (
                  <td className={`px-4 py-3 ${column.className ?? ''}`} key={column.header}>
                    {column.cell(item)}
                  </td>
                ))}
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
