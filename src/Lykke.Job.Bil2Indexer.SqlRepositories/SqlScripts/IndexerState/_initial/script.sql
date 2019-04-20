BEGIN;
create table block_headers
(
    blockchain_type   text    not null,
    number            bigint  not null,
    mined_at          date    not null,
    size              integer not null,
    transaction_count integer not null,
    previous_block_id text,
    state             integer,
    id                text    not null,
    constraint block_headers_pk
        primary key (blockchain_type, id)
);

create unique index block_headers_blockchain_type_number_uindex
    on block_headers (blockchain_type, number);

create table chain_heads
(
    blockchain_type    text   not null,
    first_block_number bigint not null,
    block_number       bigint,
    block_id           text not null,
    prev_block_id      text not null,
    constraint chain_heads_pk
        primary key (blockchain_type)
);

create table crawlers
(
    blockchain_type       text   not null,
    start_block           bigint not null,
    stop_accembling_block bigint not null,
    sequence              bigint not null,
    expected_block_number bigint not null,
    constraint crawlers_pk
        primary key (blockchain_type, start_block, stop_accembling_block)
);

COMMIT;