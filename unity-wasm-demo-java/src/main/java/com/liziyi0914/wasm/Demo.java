package com.liziyi0914.wasm;

import cn.hutool.core.io.FileUtil;
import io.github.kawamuray.wasmtime.*;
import io.github.kawamuray.wasmtime.wasi.WasiCtx;
import io.github.kawamuray.wasmtime.wasi.WasiCtxBuilder;

import java.lang.Module;
import java.nio.ByteBuffer;
import java.util.Arrays;
import java.util.Collection;
import java.util.concurrent.atomic.AtomicReference;

import static io.github.kawamuray.wasmtime.WasmValType.I32;
import static io.github.kawamuray.wasmtime.WasmValType.I64;

public class Demo {

    public static void main(String[] args) {
        byte[] file = FileUtil.readBytes("D:\\Projects\\unity-wasm-demo\\rust\\target\\wasm32-wasi\\debug\\unity_wasm_demo_rust.wasm");
        // Let the poll_word function to refer this as a placeholder of Memory because
        // we have to add the function as import before loading the module exporting Memory.
        AtomicReference<Memory> memRef = new AtomicReference<>();
        try (WasiCtx wasi = new WasiCtxBuilder().inheritStdout().inheritStderr().build();
             Store<Void> store = Store.withoutData(wasi);
             Linker linker = new Linker(store.engine());
//             Func pollWordFn = WasmFunctions.wrap(store, I64, I32, I32, (addr, len) -> {
//                 System.err.println("Address to store word: " + addr);
//                 ByteBuffer buf = memRef.get().buffer(store);
//                 String word = words[counter.getAndIncrement() % words.length];
//                 for (int i = 0; i < len && i < word.length(); i++) {
//                     buf.put(addr.intValue() + i, (byte) word.charAt(i));
//                 }
//                 return Math.min(word.length(), len);
//             });
             var module = io.github.kawamuray.wasmtime.Module.fromBinary(store.engine(), file)) {

            WasiCtx.addToLinker(linker);
//            linker.define(store, "xyz", "poll_word", Extern.fromFunc(pollWordFn));
            linker.module(store, "", module);

            try (Memory mem = linker.get(store, "", "memory").get().memory();
                 Func doWorkFn = linker.get(store, "", "fib").get().func()) {
                memRef.set(mem);
                WasmFunctions.Function1<Long, Long> fib = WasmFunctions.func(store, doWorkFn, I64, I64);
                System.out.println(fib.call(10L));
//                Consumer0 doWork = WasmFunctions.consumer(store, doWorkFn);
//                doWork.accept();
//                doWork.accept();
//                doWork.accept();
            }
        }
    }

}
