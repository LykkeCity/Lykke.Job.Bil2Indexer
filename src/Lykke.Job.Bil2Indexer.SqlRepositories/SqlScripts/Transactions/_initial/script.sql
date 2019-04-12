create extension if not exists "uuid-ossp";

create table transactions
(
    id                 uuid default uuid_generate_v1() not null,
    blockchain_type    text                            not null,
    block_id           text                            not null,
    transaction_id     text                            not null,
    transaction_number integer                         not null,
    type               integer                         not null,
    payload            jsonb                           not null,
    constraint transactions_pk
        primary key (id)
);

create index transactions_blockchain_type_block_id_index
    on transactions (blockchain_type, block_id);

create unique index transactions_btype_transaction_id_transaction_number_uind
    on transactions (blockchain_type, transaction_id, transaction_number);
COMMIT;