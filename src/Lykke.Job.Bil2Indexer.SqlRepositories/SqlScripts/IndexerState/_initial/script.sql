BEGIN;
create table block_headers
(
    blockchain_type   varchar(128)    not null,
    number            bigint  not null,
    mined_at          timestamp with time zone     not null,
    size              integer not null,
    transaction_count integer not null,
    previous_block_id varchar(256),
    state             integer,
    id                varchar(256)    not null,
    constraint block_headers_natural_key_pk
        primary key (blockchain_type, id)
);

create unique index block_headers_blockchain_type_number_uindex
    on block_headers (blockchain_type, number);

create table chain_heads
(
    blockchain_type    varchar(128)   not null,
    first_block_number bigint not null,
    block_number       bigint,
    sequence           bigint not null,
    block_id           varchar(256),
    prev_block_id      varchar(256),
    constraint chain_heads_natural_key_pk
        primary key (blockchain_type)
);

create table crawlers
(
    blockchain_type       varchar(128)   not null,
    start_block           bigint not null,
    stop_accembling_block bigint not null,
    sequence              bigint not null,
    expected_block_number bigint not null,
    constraint crawlers_natural_key_pk
        primary key (blockchain_type, start_block, stop_accembling_block)
);

COMMIT;