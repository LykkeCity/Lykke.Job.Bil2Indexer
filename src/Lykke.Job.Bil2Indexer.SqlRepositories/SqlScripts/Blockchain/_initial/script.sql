create extension if not exists "uuid-ossp";

BEGIN;
create or replace function set_numeric_value_from_string () returns trigger AS '
begin
     NEW.value= cast(NEW.value_string as numeric);
     return NEW;
end;
 ' language plpgsql;

create table coins
(
    id               uuid default uuid_generate_v1() not null,
    transaction_id   varchar(256)                            not null,
    coin_number      integer                         not null,
    asset_id         varchar(32)                            not null,
    coin_id          varchar(264)                            not null,
    asset_address    varchar(256),
    value            numeric                         not null,
    value_string     text                            not null,
    value_scale      integer                         not null,
    address          varchar(256),
    address_tag      varchar(1024),
    is_spent       boolean                         not null,
    address_tag_type smallint,
    address_nonce    numeric,
    block_number     bigint    not null,
    block_id         varchar(256)    not null,
    constraint coins_pk
        primary key (id)
);
 
create unique index coins_natural_key_index
    on coins (coin_id)  tablespace fast_space;

create index coins_transaction_id_index
	on coins (transaction_id)  tablespace fast_space;

create index coins_address_coin_id_index
	on coins (address, coin_id)  tablespace fast_space where is_spent =false;


create  trigger set_numeric_value_from_string_trigger before insert or update
     on coins for each row
     execute procedure set_numeric_value_from_string ();

create table fees
(
    id              uuid default uuid_generate_v1() not null,
    block_id        varchar(256)                            not null,
    transaction_id  varchar(256)                            not null,
    asset_id        varchar(32)                            not null,
    asset_address   varchar(256),
    value           numeric                         not null,
    value_string    text                            not null,
    value_scale     integer                         not null,
    constraint fees_pk
        primary key (id)
);

create unique index fees_natural_key_index_1
    on fees (transaction_id, asset_id)  tablespace fast_space
where asset_address is null ;

create unique index fees_natural_key_index_2
    on fees (transaction_id, asset_id, asset_address)  tablespace fast_space
where asset_address is not null;

create index fees_block_id_index
	on fees (block_id)   tablespace fast_space;
    
create  trigger set_numeric_value_from_string_trigger before insert or update
     on fees for each row
     execute procedure set_numeric_value_from_string ();

create table balance_actions
(
    id              uuid default uuid_generate_v1(),
    block_id        varchar(256)    not null,
    block_number    integer not null,
    asset_id        varchar(32)    not null,
    asset_address   varchar(256),
    transaction_id  varchar(256)    not null,
    value           numeric not null,
    value_string    text    not null,
    value_scale     integer not null,
    address         varchar(256)    not null
);

create unique index balance_actions_natural_key_index_1
    on balance_actions (transaction_id, address, asset_id)   tablespace fast_space
    where asset_address is null;

create unique index balance_actions_natural_key_index_2
    on balance_actions (transaction_id, address, asset_id, asset_address)   tablespace fast_space
    where asset_address is not null;
    
create index balance_actions_block_id
    on balance_actions (block_id);

create index query_covered_by_address
    on balance_actions (address, block_number desc, asset_id, asset_address, value)  tablespace fast_space;

create  trigger set_numeric_value_from_string_trigger before insert or update
     on balance_actions for each row
     execute procedure set_numeric_value_from_string ();



create table assets
(
    id              varchar(296)                            not null,
    asset_id        varchar(32)                            not null,
    asset_address   varchar(256),
	scale int not null,
	constraint assets_natural_key_pk
		primary key (id)
);

COMMIT;