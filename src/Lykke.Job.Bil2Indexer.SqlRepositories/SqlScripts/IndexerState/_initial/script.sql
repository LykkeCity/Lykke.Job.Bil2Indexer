BEGIN;
create table block_headers
(
    number            bigint  not null,
    mined_at          timestamp with time zone     not null,
    size              integer not null,
    transaction_count integer not null,
    previous_block_id varchar(256),
    state             integer,
    id                varchar(256)    not null,
    constraint block_headers_natural_key_pk
        primary key (id)
);

create unique index block_headers_number_uindex
    on block_headers (mined_at)   tablespace fast_space;

    
create index block_headers_mined_at_index
    on block_headers (number)   tablespace fast_space;

create table chain_heads
(
    id                 varchar(128) not null,
    first_block_number bigint not null,
    block_number       bigint,
    sequence           bigint not null,
    block_id           varchar(256),
    prev_block_id      varchar(256),
    constraint chain_heads_natural_key_pk
        primary key (id)
);

create table crawlers
(
    start_block           bigint not null,
    stop_accembling_block bigint not null,
    sequence              bigint not null,
    expected_block_number bigint not null,
    constraint crawlers_natural_key_pk
        primary key (start_block, stop_accembling_block)
);

COMMIT;