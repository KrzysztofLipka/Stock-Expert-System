import './Table.css';

export type ColumnDefinitionType<T, K extends keyof T> = {
    key: K;
    header: string;
}

type TableProps<T, K extends keyof T> = {
    data: Array<T>;
    columns: Array<ColumnDefinitionType<T, K>>;
    actionOnRowClick?: (row: T) => T | void;
}

type TableRowsProps<T, K extends keyof T> = {
    data: Array<T>;
    columns: Array<ColumnDefinitionType<T, K>>;
    actionOnRowClick?: (row: T) => T | void;
}

type TableHeaderProps<T, K extends keyof T> = {
    columns: Array<ColumnDefinitionType<T, K>>;
}

const TableHeader = <T, K extends keyof T>({ columns }: TableHeaderProps<T, K>): JSX.Element => {
    const headers = columns.map((column, index) => {

        return (
            <th
                key={`headerCell-${index}`}
                className={'table-header'}
            >
                {column.header}
            </th>
        );
    });

    return (
        <thead>
            <tr>{headers}</tr>
        </thead>
    );
};

const TableRows = <T, K extends keyof T>({ data, columns, actionOnRowClick }: TableRowsProps<T, K>): JSX.Element => {
    //console.log(actionOnRowClick);
    const rows = data.map((row, index) => {
        return (
            <tr key={`row-${index}`} onClick={actionOnRowClick ? () => actionOnRowClick(row) : () => null}>
                {columns.map((column, cellIndex) => {
                    return (
                        <td key={`cell-${cellIndex}`}>
                            {row[column.key]}
                        </td>
                    );
                }
                )}
            </tr>
        );
    });

    return (
        <tbody>
            {rows}
        </tbody>
    );
};


export const Table = <T, K extends keyof T>({ data, columns, actionOnRowClick }: TableProps<T, K>): JSX.Element => {
    return (
        <table className='table'>
            <TableHeader columns={columns} />
            <TableRows
                data={data}
                columns={columns}
                actionOnRowClick={actionOnRowClick}
            />
        </table>
    );
};
