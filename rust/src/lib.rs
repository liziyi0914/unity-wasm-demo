mod core;

#[no_mangle]
pub extern "C" fn fib(n: u64) -> u64 {
    core::fib(n)
}
