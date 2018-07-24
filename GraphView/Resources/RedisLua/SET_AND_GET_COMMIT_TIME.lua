﻿-- A CAS operation to set the tx entry's commit time with the local commit time
-- if the commit_time in the tx entry has not been set, take the max(try_commit_time, commit_lower_bound)
-- as the commit time and set it.
-- usage: eval lua_script 1 txId try_commit_time -1

local try_commit_time = ARGV[1]
-- the uploaded negative_one to be a return value in some cases
local negative_one = ARGV[2]

local tx_entry = redis.call('HMGET', KEYS[1], 'commit_time', 'commit_lower_bound')

if not tx_entry then
    return negative_one
end

local commit_time = tx_entry[1]
local commit_lower_bound = tx_entry[2]

if commit_time == negative_one then
	commit_time = try_commit_time
	if string.byte(commit_lower_bound) > string.byte(try_commit_time) then
		commit_time = commit_lower_bound
	end

    local ret = redis.call("HSET", KEYS[1], "commit_time", commit_time)
    if ret == 0 then
        return commit_time
    end
    return negative_one
end
return negative_one