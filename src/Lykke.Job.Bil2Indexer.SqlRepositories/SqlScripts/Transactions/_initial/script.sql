create extension if not exists "uuid-ossp";

create table transactions
(
    id                 uuid default uuid_generate_v1() not null,
    blockchain_type    varchar(128)                            not null,
    block_id           varchar(256)                            not null,
    transaction_id     varchar(256)                            not null,
    transaction_number integer                         not null,
    type               integer                         not null,
    payload            jsonb                           not null,
    constraint transactions_pk
        primary key (id)
);

create index transactions_blockchain_type_block_id_index
    on transactions (blockchain_type, block_id);

create unique index transactions_natural_key_index
    on transactions (blockchain_type, transaction_id);


COMMIT;