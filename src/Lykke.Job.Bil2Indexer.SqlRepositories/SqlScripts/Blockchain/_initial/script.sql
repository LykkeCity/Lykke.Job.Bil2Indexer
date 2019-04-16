create extension if not exists "uuid-ossp";
BEGIN;
create table coins
(
    id               uuid default uuid_generate_v1() not null,
    blockchain_type  text                            not null,
    transaction_id   text                            not null,
    coin_number      integer                         not null,
    asset_id         text                         not null,
    asset_address    text,
    value            numeric                         not null,
    value_scale      integer                         not null,
    address          text,
    address_tag      text,
    is_deleted       boolean                             not null,
    address_tag_type integer,
    address_nonce    numeric,
    is_spent         boolean                             not null,
    constraint coins_pk
        primary key (id)
);

create unique index coins_blockchain_type_transaction_id_coin_number_uindex
    on coins (blockchain_type, transaction_id, coin_number);

create table fees
(
    id              uuid default uuid_generate_v1() not null,
    blockchain_type text                            not null,
    block_id        text                            not null,
    transaction_id  text                            not null,
    asset_id        text                            not null,
    asset_address   text,
    value           numeric                         not null,
    value_scale     integer                         not null,
    constraint fees_pk
        primary key (id)
);

create unique index fees_blockchain_type_transaction_id_asset_id_uindex
    on fees (blockchain_type, transaction_id, asset_id);

create index fees_blockchain_type_block_id_index
	on fees (blockchain_type, block_id);

create table balance_actions
(
    id              uuid default uuid_generate_v1(),
    blockchain_type text    not null,
    block_id        text    not null,
    block_number    integer not null,
    asset_id        text    not null,
    asset_address   text,
    transaction_id  text    not null,
    value           numeric not null,
    value_scale     integer not null,
    address         text    not null
);

create unique index balance_actions_blockchain_type_asset_id_transaction_id_uindex
    on balance_actions (blockchain_type, asset_id, transaction_id);

create index balance_actions_blockchain_type_address_block_number_asset_id_v
    on balance_actions (blockchain_type asc, address asc, block_number desc, asset_id asc, value asc);


    
COMMIT;